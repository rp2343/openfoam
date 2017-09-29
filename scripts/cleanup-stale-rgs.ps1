Param
(
      [Parameter (Mandatory= $true)]
      [String] $tag,

      [Parameter (Mandatory= $true)]
      [int] $ageLimitInMinutes
)

Write-Output "Logging in..."

$connectionName = "AzureRunAsConnection"
try
{
    # Get the connection "AzureRunAsConnection "
    $servicePrincipalConnection=Get-AutomationConnection -Name $connectionName         

    "Logging in to Azure..."
    Add-AzureRmAccount `
        -ServicePrincipal `
        -TenantId $servicePrincipalConnection.TenantId `
        -ApplicationId $servicePrincipalConnection.ApplicationId `
        -CertificateThumbprint $servicePrincipalConnection.CertificateThumbprint 
}
catch {
    if (!$servicePrincipalConnection)
    {
        $ErrorMessage = "Connection $connectionName not found."
        throw $ErrorMessage
    } else{
        Write-Error -Message $_.Exception
        throw $_.Exception
    }
}

#---------------------
Write-Output "Logged in..."

[array]$rgs = Find-AzureRmResourceGroup  -Tag @{ "workload"=$tag }
Write-Output "Stale RGs found: $($rgs.Length)"

foreach ($rg in $rgs)
{

    [array]$ops = Get-AzureRmLog -ResourceGroup $rg.name -MaxEvents 100000 -WarningAction Ignore | Where-Object OperationName -eq Microsoft.Resources/subscriptions/resourcegroups/write
    $createTimestamp = $ops[0].EventTimestamp
    $currentTime = (Get-Date).ToUniversalTime()
    $ageInMinutes = (($currentTime)-($createTimestamp)).TotalMinutes
    
    if ($ageInMinutes -ge $ageLimitInMinutes)
    {
        Write-Output "Deleting $($rg.name) aged $ageInMinutes minutes"
        Remove-AzureRmResourceGroup -Name $rg.name -Force
    }

}


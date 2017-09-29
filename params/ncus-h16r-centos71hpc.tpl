my_uid=$(uuidgen | cut -c1-6)

githubUser=$(git config --get remote.origin.url | cut -d'/' -f4)
githubBranch=$(git status | sed -n 's/[# ]*On branch \([^ ]\+\)/\1/p')

resource_group=NAME-azhpc-${my_uid}
location="North Central US"
vmSku=Standard_H16r
vmssName=az${my_uid}
computeNodeImage=CentOS-HPC_7.3
instanceCount=4
rsaPublicKey=$(cat ~/.ssh/id_rsa.pub)

numberOfNodesToTest="8 16"
processesPerNode=16
podKey=$(az keyvault secret show --name podkey --vault-name NinaVault | jq '.value' -r)

linpack_N=69120
linpack_P=1
linpack_Q=2
linpack_NB=192
linpack_Peak=691.2

azLogin=
azPassword=
azTenant=

rootLogDir='.'

logToStorage=true
logStorageAccountName=ninalogs
logStorageContainerName=results
logStoragePath=
logStorageSasKey="?sv=2017-04-17&ss=bfqt&srt=sco&sp=rw&se=2027-09-27T10:07:48Z&st=2017-09-27T02:07:48Z&spr=https&sig=IXNV8%2B2mGTuWoRvn5ZcHpdzY9MtEeqN8ootSz%2BLez2w%3D"
cosmos_account=ninadb
cosmos_database=Nina
cosmos_collection=Results
cosmos_key=$(az keyvault secret show --name cosmoskey --vault-name NinaVault | jq '.value' -r)

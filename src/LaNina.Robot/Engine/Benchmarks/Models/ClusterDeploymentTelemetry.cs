using Newtonsoft.Json;

namespace LaNina.Robot.Engine.Benchmarks.Models
{
    internal class ClusterDeploymentTelemetry
    {
        [JsonProperty("sshretries")]
        public int SshRetries { get; set; }
        
        [JsonProperty("status")]
        public string Status { get; set; }
    }
}
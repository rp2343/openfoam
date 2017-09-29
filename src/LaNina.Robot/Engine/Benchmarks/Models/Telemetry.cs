using System;
using Newtonsoft.Json;

namespace LaNina.Robot.Engine.Benchmarks.Models
{
    internal class Telemetry
    {
        [JsonProperty("id")]
        public string Id { get; set; }
        
        [JsonProperty("subscription")]
        public string Subscription { get; set; }
        
        [JsonProperty("location")]
        public string Location { get; set; }
        
        [JsonProperty("resourceGroup")]
        public string ResourceGroup { get; set; }
        
        [JsonProperty("vmSize")]
        public string VmSize { get; set; }
        
        [JsonProperty("computeNodeImage")]
        public string ComputeNodeImage { get; set; }
        
        [JsonProperty("instanceCount")]
        public int InstanceCount { get; set; }
        
        [JsonProperty("provisioningState")]
        public string ProvisioningState { get; set; }
        
        [JsonProperty("deploymentTimestamp")]
        public DateTimeOffset DeploymentTimestamp { get; set; }
        
        [JsonProperty("clusterDeployment")]
        public ClusterDeploymentTelemetry ClusterDeploymentTelemetry { get; set; }

        [JsonProperty("stream")]
        public StreamTelemetry StreamTelemetry { get; set; }

        // TODO: Verify
        [JsonProperty("singlehpl")]
        public SingleHplTelemetry SingleHplTelemetry { get; set; }

        [JsonProperty("ringpingpong")]
        public RingPingPongTelemetry RingPingPongTelemetry { get; set; }

        [JsonProperty("benchmark")]
        public BenchmarkTelemetry BenchmarkTelemetry { get; set; }
    }
}
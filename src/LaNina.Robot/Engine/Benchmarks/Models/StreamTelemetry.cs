using Newtonsoft.Json;

namespace LaNina.Robot.Engine.Benchmarks.Models
{
    internal class StreamTelemetry
    {
        [JsonProperty("results")]
        public StreamTelemetryResult[] Results { get; set; }
    }
}
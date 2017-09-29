using Newtonsoft.Json;

namespace LaNina.Robot.Engine.Benchmarks.Models
{
    internal class StreamTelemetryResult
    {
        [JsonProperty("hostname")]
        public string Hostname { get; set; }

        [JsonProperty("triad")]
        public decimal Triad { get; set; }
    }
}
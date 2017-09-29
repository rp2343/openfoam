using Newtonsoft.Json;

namespace LaNina.Robot.Engine.Benchmarks.Models
{
    internal class RingPingPongTelemetry
    {
        [JsonProperty("results")]
        public RingPingPongResult[] Results { get; set; }
    }
}
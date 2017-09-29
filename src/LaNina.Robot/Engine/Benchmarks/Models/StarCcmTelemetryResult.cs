using Newtonsoft.Json;

namespace LaNina.Robot.Engine.Benchmarks.Models
{
    internal class StarCcmTelemetryResult
    {
        [JsonProperty("avgelapsedtime")]
        public decimal AvgElapsedTimeInSec { get; set; }

        [JsonProperty("workers")]
        public int Workers { get; set; }
    }
}
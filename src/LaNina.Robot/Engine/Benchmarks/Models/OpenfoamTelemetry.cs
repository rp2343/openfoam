using Newtonsoft.Json;

namespace LaNina.Robot.Engine.Benchmarks.Models
{
    internal class OpenfoamTelemetry
    {
        [JsonProperty("results")]
        public OpenfoamTelemetryResult[] Results { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("version")]
        public string Version { get; set; }
    }
}
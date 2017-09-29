using Newtonsoft.Json;

namespace LaNina.Robot.Engine.Benchmarks.Models
{
    internal class BenchmarkTelemetry
    {
        [JsonProperty("openfoam")]
        public OpenfoamTelemetry OpenfoamTelemetry { get; set; }
    }
}
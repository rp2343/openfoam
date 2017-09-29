using Newtonsoft.Json;

namespace LaNina.Robot.Engine.Benchmarks.Models
{
    internal class SingleHplTelemetryResult
    {
        [JsonProperty("hostname")]
        public string Hostname { get; set; }
        
        [JsonProperty("duration")]
        public double DurationInSeconds { get; set; }
        
        [JsonProperty("gflops")]
        [JsonConverter(typeof(DecimalConverter))]
        public decimal Gflops { get; set; }
    }
}
using Newtonsoft.Json;

namespace LaNina.Robot.Engine.Benchmarks.Models
{
    internal class SingleHplTelemetry
    {
        [JsonProperty("vmSize")]
        public string VmSize { get; set; }
        
        [JsonProperty("N")]
        public int N { get; set; }
        
        [JsonProperty("P")]
        public int P { get; set; }
        
        [JsonProperty("Q")]
        public int Q { get; set; }
        
        // ReSharper disable once InconsistentNaming
        [JsonProperty("NB")]
        public int NB { get; set; }
        
        [JsonProperty("PeakGflops")]
        public double PeakGflops { get; set; }
        
        [JsonProperty("results")]
        public SingleHplTelemetryResult[] Results { get; set; }
    }
}
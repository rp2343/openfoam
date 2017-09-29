using Newtonsoft.Json;

namespace LaNina.Robot.Engine.Benchmarks.Models
{
    internal class RingPingPongResult
    {
        [JsonProperty("src")]
        public string Src { get; set; }

        [JsonProperty("dst")]
        public string Dst { get; set; }

        [JsonProperty("bytes")]
        public int Bytes { get; set; }

        [JsonProperty("repetitions")]
        public int Repetitions { get; set; }

        [JsonProperty("t_usec")]
        public double Tusec { get; set; }

        [JsonProperty("Mbytes_sec")]
        public double MbytesSec{ get; set; }
    }
}
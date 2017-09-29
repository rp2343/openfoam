namespace LaNina.Robot.Engine.Benchmarks.Models
{
    internal class BashResult
    {
        public string Stdout { get; set; }
        public string Stderr { get; set; }
        public int ExitCode { get; set; }
    }
}
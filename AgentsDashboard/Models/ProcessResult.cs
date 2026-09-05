namespace AgentsDashboard.Models
{
    public class ProcessResult
    {
        public int ExitCode { get; set; }
        public string Output { get; set; } = "";

        public bool Success => ExitCode == 0;
    }
}

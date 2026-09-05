namespace AgentsDashboard.Models
{
    public class GitInfo
    {
        public string Branch { get; set; } = "";
        public int CommitsAhead { get; set; }
        public bool HasChanges { get; set; }
        public string LastCommit { get; set; } = "";
        public bool Exists { get; set; }
    }
}

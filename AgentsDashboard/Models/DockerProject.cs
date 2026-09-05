using System.Text.Json.Serialization;

namespace AgentsDashboard.Models
{
    public class DockerProject
    {
        [JsonPropertyName("Name")]
        public string Name { get; set; } = "";

        [JsonPropertyName("Status")]
        public string Status { get; set; } = "";

        [JsonPropertyName("ConfigFiles")]
        public string ConfigFiles { get; set; } = "";

        public bool IsRunning => Status.Contains("running");

        public bool BelongsTo(string worktreePath)
        {
            return ConfigFiles.StartsWith(worktreePath, StringComparison.OrdinalIgnoreCase);
        }
    }
}

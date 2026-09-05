using System.Text.Json.Serialization;

namespace AgentsDashboard.Models
{
    public class Feature
    {
        public string Id { get; set; } = Guid.NewGuid().ToString("N");
        public string Title { get; set; } = "";
        public string Description { get; set; } = "";
        public FeatureStatus Status { get; set; } = FeatureStatus.Erstellt;
        public string BranchName { get; set; } = "";
        public string WorktreePath { get; set; } = "";
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? DoneAt { get; set; }
        public List<string> Screenshots { get; set; } = new();

        public Feature() { }

        public Feature(string title, string description)
        {
            Title = title;
            Description = description;
        }

        [JsonIgnore]
        public string FolderName => Path.GetFileName(WorktreePath);
    }
}

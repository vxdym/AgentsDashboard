using System.Text;
using AgentsDashboard.Models;
using AgentsDashboard.Services;
using Microsoft.AspNetCore.Components;

namespace AgentsDashboard.Components.Pages
{
    public partial class Changelog : ComponentBase
    {
        [Inject] private FeatureStore Store { get; set; } = null!;
        [Inject] private IWebHostEnvironment Env { get; set; } = null!;

        private List<Feature> done = new();
        private string? exportPath;

        protected override void OnInitialized()
        {
            done = Store.GetAll()
                .Where(f => f.Status == FeatureStatus.Fertig)
                .OrderByDescending(f => f.DoneAt)
                .ToList();
        }

        private void Export()
        {
            var sb = new StringBuilder();
            sb.AppendLine("# Salonary Changelog");
            sb.AppendLine();

            foreach (var feature in done)
            {
                sb.AppendLine($"## {feature.Title} ({feature.DoneAt:dd.MM.yyyy})");
                sb.AppendLine();
                sb.AppendLine($"Branch: `{feature.BranchName}`");
                sb.AppendLine();
                if (!string.IsNullOrWhiteSpace(feature.Description))
                {
                    sb.AppendLine(feature.Description);
                    sb.AppendLine();
                }
            }

            var dir = Path.Combine(Env.ContentRootPath, "Data");
            Directory.CreateDirectory(dir);
            exportPath = Path.Combine(dir, "CHANGELOG.md");
            File.WriteAllText(exportPath, sb.ToString());
        }
    }
}

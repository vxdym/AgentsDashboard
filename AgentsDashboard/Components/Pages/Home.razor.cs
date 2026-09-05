using AgentsDashboard.Models;
using AgentsDashboard.Services;
using Microsoft.AspNetCore.Components;

namespace AgentsDashboard.Components.Pages
{
    public partial class Home : ComponentBase
    {
        [Inject] private FeatureStore Store { get; set; } = null!;
        [Inject] private GitService Git { get; set; } = null!;
        [Inject] private NavigationManager Navigation { get; set; } = null!;

        private List<Feature> features = new();

        protected override async Task OnInitializedAsync()
        {
            await ImportWorktrees();
            features = Store.GetAll();
        }

        private async Task ImportWorktrees()
        {
            foreach (var path in await Git.ListWorktrees())
            {
                if (Store.GetByWorktree(path) != null)
                    continue;

                var feature = new Feature(Path.GetFileName(path), "");
                feature.WorktreePath = path;
                feature.BranchName = await Git.GetBranch(path);
                feature.Status = FeatureStatus.InArbeit;
                feature.CreatedAt = Directory.GetCreationTime(path);
                Store.Add(feature);
            }
        }

        private void OpenFeature(Feature feature)
        {
            Navigation.NavigateTo($"feature/{feature.Id}");
        }
    }
}

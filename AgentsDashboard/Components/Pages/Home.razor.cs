using AgentsDashboard.Models;
using AgentsDashboard.Services;
using Microsoft.AspNetCore.Components;

namespace AgentsDashboard.Components.Pages
{
    public partial class Home : ComponentBase
    {
        [Inject] private FeatureStore Store { get; set; } = null!;
        [Inject] private GitService Git { get; set; } = null!;
        [Inject] private DockerService Docker { get; set; } = null!;
        [Inject] private NavigationManager Navigation { get; set; } = null!;

        private List<Feature> features = new();
        private List<IGrouping<string, Feature>> groups = new();
        private Dictionary<string, GitInfo> gitInfos = new();
        private List<DockerProject> runningProjects = new();

        protected override async Task OnInitializedAsync()
        {
            await ImportWorktrees();
            features = Store.GetAll();
            groups = features.GroupBy(GroupName).OrderBy(g => GroupOrder(g.Key)).ToList();
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (!firstRender)
                return;

            runningProjects = (await Docker.ListProjects()).Where(p => p.IsRunning).ToList();
            StateHasChanged();

            foreach (var feature in features)
            {
                gitInfos[feature.Id] = await Git.GetInfo(feature.WorktreePath);
                StateHasChanged();
            }
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

        private static string GroupName(Feature feature)
        {
            switch (feature.Status)
            {
                case FeatureStatus.BereitZurPruefung:
                    return "Bereit zur Prüfung";
                case FeatureStatus.Fertig:
                    return "Fertig";
                case FeatureStatus.Erstellt:
                case FeatureStatus.InPlanung:
                    return "Geplant";
                default:
                    return "In Entwicklung";
            }
        }

        private static int GroupOrder(string name)
        {
            switch (name)
            {
                case "Bereit zur Prüfung": return 0;
                case "In Entwicklung": return 1;
                case "Geplant": return 2;
                default: return 3;
            }
        }

        private void OpenFeature(Feature feature)
        {
            Navigation.NavigateTo($"feature/{feature.Id}");
        }
    }
}

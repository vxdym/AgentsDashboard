using AgentsDashboard.Models;
using AgentsDashboard.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

namespace AgentsDashboard.Components.Pages
{
    public partial class NewFeature : ComponentBase
    {
        [Inject] private FeatureStore Store { get; set; } = null!;
        [Inject] private GitService Git { get; set; } = null!;
        [Inject] private NavigationManager Navigation { get; set; } = null!;

        private string title = "";
        private string description = "";
        private List<IBrowserFile> files = new();
        private bool busy;
        private string? error;

        private void OnFilesSelected(InputFileChangeEventArgs e)
        {
            files = e.GetMultipleFiles(20).ToList();
        }

        private async Task Create()
        {
            busy = true;
            error = null;

            try
            {
                var feature = new Feature(title.Trim(), description.Trim());
                feature.BranchName = GitService.MakeName(title);
                feature.WorktreePath = await Git.CreateWorktree(feature.BranchName);

                foreach (var file in files)
                {
                    using var stream = file.OpenReadStream(20 * 1024 * 1024);
                    var path = await Store.SaveScreenshot(feature.Id, file.Name, stream);
                    feature.Screenshots.Add(path);
                }

                Store.Add(feature);
                Navigation.NavigateTo($"feature/{feature.Id}");
            }
            catch (Exception ex)
            {
                error = ex.Message;
            }
            finally
            {
                busy = false;
            }
        }
    }
}

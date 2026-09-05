using AgentsDashboard.Models;
using AgentsDashboard.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

namespace AgentsDashboard.Components.Pages
{
    public partial class FeatureDetails : ComponentBase
    {
        [Parameter] public string Id { get; set; } = "";

        [Inject] private FeatureStore Store { get; set; } = null!;
        [Inject] private GitService Git { get; set; } = null!;
        [Inject] private DockerService Docker { get; set; } = null!;
        [Inject] private TerminalService Terminal { get; set; } = null!;
        [Inject] private AgentPrompts Prompts { get; set; } = null!;
        [Inject] private NavigationManager Navigation { get; set; } = null!;

        private Feature? feature;
        private GitInfo? gitInfo;
        private bool worktreeExists;
        private bool saved;
        private bool confirmRemove;
        private string? error;

        private bool dockerRunning;
        private bool dockerBusy;
        private List<string> dockerLog = new();

        protected override async Task OnInitializedAsync()
        {
            feature = Store.GetById(Id);
            if (feature == null)
                return;

            worktreeExists = Directory.Exists(feature.WorktreePath);
            await LoadGitInfo();
            await LoadDockerState();
        }

        private async Task LoadGitInfo()
        {
            gitInfo = null;
            gitInfo = await Git.GetInfo(feature!.WorktreePath);
        }

        private async Task LoadDockerState()
        {
            dockerRunning = await Docker.GetRunning(feature!.WorktreePath) != null;
        }

        private void OnStatusChanged(ChangeEventArgs e)
        {
            SetStatus(Enum.Parse<FeatureStatus>(e.Value?.ToString() ?? "Erstellt"));
        }

        private void SetStatus(FeatureStatus status)
        {
            feature!.Status = status;
            feature.DoneAt = status == FeatureStatus.Fertig ? DateTime.Now : null;
            Store.Save();
        }

        private async Task SaveDescription()
        {
            Store.Save();
            saved = true;
            await Task.Delay(1500);
            saved = false;
        }

        private async Task AddScreenshots(InputFileChangeEventArgs e)
        {
            foreach (var file in e.GetMultipleFiles(20))
            {
                using var stream = file.OpenReadStream(20 * 1024 * 1024);
                var path = await Store.SaveScreenshot(feature!.Id, file.Name, stream);
                feature.Screenshots.Add(path);
            }
            Store.Save();
        }

        private void OpenVsCode()
        {
            Try(() => Terminal.OpenVsCode(feature!.WorktreePath));
        }

        private void OpenClaude()
        {
            Try(() => Terminal.OpenClaude(feature!.WorktreePath));
        }

        private void OpenCodex()
        {
            Try(() => Terminal.OpenCodex(feature!.WorktreePath));
        }

        private void StartAgent(AgentType type)
        {
            Try(() =>
            {
                var prompt = Prompts.Build(feature!, type);
                var name = $"{feature.BranchName}-{type}".ToLower();
                Terminal.OpenClaudeWithPrompt(feature.WorktreePath, name, prompt, type == AgentType.Planung);

                if (type == AgentType.Planung && feature.Status == FeatureStatus.Erstellt)
                    SetStatus(FeatureStatus.InPlanung);
                if (type == AgentType.Implementierung && feature.Status < FeatureStatus.InArbeit)
                    SetStatus(FeatureStatus.InArbeit);
            });
        }

        private async Task StartDocker()
        {
            await RunDocker(log => Docker.Start(feature!.WorktreePath, log));
        }

        private async Task StopDocker()
        {
            await RunDocker(log => Docker.Stop(feature!.WorktreePath, log));
        }

        private async Task RunDocker(Func<Action<string>, Task<ProcessResult>> action)
        {
            dockerBusy = true;
            dockerLog.Clear();
            error = null;

            void Log(string line)
            {
                dockerLog.Add(line);
                InvokeAsync(StateHasChanged);
            }

            try
            {
                var result = await action(Log);
                if (!result.Success)
                    error = "Docker-Befehl fehlgeschlagen (Exit-Code " + result.ExitCode + ").";
            }
            catch (Exception ex)
            {
                error = ex.Message;
            }
            finally
            {
                dockerBusy = false;
                await LoadDockerState();
            }
        }

        private async Task Remove()
        {
            error = null;

            if (worktreeExists)
            {
                var result = await Git.RemoveWorktree(feature!.WorktreePath, feature.BranchName);
                if (!result.Success)
                {
                    error = result.Output;
                    confirmRemove = false;
                    return;
                }
            }

            Store.Remove(feature!);
            Navigation.NavigateTo("");
        }

        private void Try(Action action)
        {
            error = null;
            try
            {
                action();
            }
            catch (Exception ex)
            {
                error = ex.Message;
            }
        }
    }
}

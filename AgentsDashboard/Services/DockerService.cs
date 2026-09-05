using System.Text.Json;
using AgentsDashboard.Models;

namespace AgentsDashboard.Services
{
    public class DockerService
    {
        private readonly ProcessRunner runner;

        public DockerService(ProcessRunner runner)
        {
            this.runner = runner;
        }

        public async Task<List<DockerProject>> ListProjects()
        {
            var result = await runner.Run("docker", "compose ls -a --format json", Directory.GetCurrentDirectory());
            if (!result.Success || string.IsNullOrWhiteSpace(result.Output))
                return new();

            try
            {
                return JsonSerializer.Deserialize<List<DockerProject>>(result.Output) ?? new();
            }
            catch
            {
                return new();
            }
        }

        public async Task<DockerProject?> GetRunning(string worktreePath)
        {
            var projects = await ListProjects();
            return projects.FirstOrDefault(p => p.IsRunning && p.BelongsTo(worktreePath));
        }

        public async Task<ProcessResult> Start(string worktreePath, Action<string> log)
        {
            var projects = await ListProjects();

            foreach (var project in projects.Where(p => p.IsRunning && !p.BelongsTo(worktreePath)))
            {
                log($"> docker compose -p {project.Name} down");
                var down = await runner.Run("docker", $"compose -p {project.Name} down", worktreePath, log);
                if (!down.Success)
                    return down;
            }

            log("> docker compose up -d --build");
            return await runner.Run("docker", "compose up -d --build", worktreePath, log);
        }

        public async Task<ProcessResult> Stop(string worktreePath, Action<string> log)
        {
            log("> docker compose down");
            return await runner.Run("docker", "compose down", worktreePath, log);
        }
    }
}

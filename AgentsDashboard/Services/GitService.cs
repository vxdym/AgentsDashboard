using System.Text;
using AgentsDashboard.Models;

namespace AgentsDashboard.Services
{
    public class GitService
    {
        private readonly ProcessRunner runner;

        public string RepoPath { get; }
        public string WorktreeRoot { get; }

        public GitService(IConfiguration config, ProcessRunner runner)
        {
            this.runner = runner;
            RepoPath = config["Salonary:RepoPath"] ?? "";
            WorktreeRoot = config["Salonary:WorktreeRoot"] ?? "";
        }

        public async Task<List<string>> ListWorktrees()
        {
            var result = await runner.Run("git", "worktree list --porcelain", RepoPath);
            var paths = new List<string>();

            foreach (var line in result.Output.Split('\n'))
            {
                if (!line.StartsWith("worktree "))
                    continue;

                var path = Path.GetFullPath(line.Substring(9).Trim());
                if (!string.Equals(path, Path.GetFullPath(RepoPath), StringComparison.OrdinalIgnoreCase))
                    paths.Add(path);
            }

            return paths;
        }

        public async Task<string> CreateWorktree(string name)
        {
            var path = Path.Combine(WorktreeRoot, name);

            if (Directory.Exists(path))
                throw new Exception($"Ordner {path} existiert bereits.");

            var fetch = await runner.Run("git", "fetch origin master", RepoPath);
            if (!fetch.Success)
                throw new Exception("git fetch fehlgeschlagen:\n" + fetch.Output);

            var add = await runner.Run("git", $"worktree add -b {name} \"{path}\" origin/master", RepoPath);
            if (!add.Success)
                throw new Exception("git worktree add fehlgeschlagen:\n" + add.Output);

            var env = Path.Combine(RepoPath, ".env");
            if (File.Exists(env))
                File.Copy(env, Path.Combine(path, ".env"));

            return path;
        }

        public async Task<ProcessResult> RemoveWorktree(string path, string branch)
        {
            var result = await runner.Run("git", $"worktree remove --force \"{path}\"", RepoPath);
            if (!result.Success)
                return result;

            return await runner.Run("git", $"branch -D {branch}", RepoPath);
        }

        public async Task<string> GetBranch(string path)
        {
            var result = await runner.Run("git", "rev-parse --abbrev-ref HEAD", path);
            return result.Output.Trim();
        }

        public async Task<GitInfo> GetInfo(string path)
        {
            var info = new GitInfo();

            if (!Directory.Exists(path))
                return info;

            info.Exists = true;
            info.Branch = await GetBranch(path);

            var ahead = await runner.Run("git", "rev-list --count origin/master..HEAD", path);
            int.TryParse(ahead.Output.Trim(), out var count);
            info.CommitsAhead = count;

            var status = await runner.Run("git", "status --porcelain", path);
            info.HasChanges = status.Output.Length > 0;

            var log = await runner.Run("git", "log -1 --format=%s", path);
            info.LastCommit = log.Output.Trim();

            return info;
        }

        public static string MakeName(string title)
        {
            var text = title.Trim()
                .Replace("ä", "ae").Replace("ö", "oe").Replace("ü", "ue").Replace("ß", "ss")
                .Replace("Ä", "Ae").Replace("Ö", "Oe").Replace("Ü", "Ue");

            var sb = new StringBuilder();
            foreach (var c in text)
            {
                if (char.IsLetterOrDigit(c))
                    sb.Append(c);
                else if (c == ' ' || c == '-' || c == '_')
                    sb.Append('-');
            }

            var name = sb.ToString().Trim('-');
            while (name.Contains("--"))
                name = name.Replace("--", "-");

            return "Salonary-" + name;
        }
    }
}

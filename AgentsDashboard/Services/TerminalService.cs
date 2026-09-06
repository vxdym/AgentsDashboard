using System.Text;

namespace AgentsDashboard.Services
{
    public class TerminalService
    {
        private readonly ProcessRunner runner;
        private readonly string promptDir;

        public TerminalService(ProcessRunner runner, IWebHostEnvironment env)
        {
            this.runner = runner;
            promptDir = Path.Combine(env.ContentRootPath, "Data", "prompts");
        }

        public void OpenVsCode(string path)
        {
            runner.StartHidden("cmd", $"/c code \"{path}\"", path);
        }

        public void OpenClaude(string path)
        {
            OpenTerminal(path, "claude");
        }

        public void OpenCodex(string path)
        {
            OpenTerminal(path, "codex");
        }

        public void OpenPowerShell(string path)
        {
            OpenTerminal(path);
        }

        public void OpenClaudeWithPrompt(string path, string name, string prompt, bool planMode = false)
        {
            Directory.CreateDirectory(promptDir);
            var file = Path.Combine(promptDir, $"{name}.md");
            File.WriteAllText(file, prompt, Encoding.UTF8);

            var mode = planMode ? "--permission-mode plan " : "";
            OpenTerminal(path, $"claude {mode}(Get-Content -Raw '{file}')");
        }

        private void OpenTerminal(string path, string command = "")
        {
            string hasCommand = string.IsNullOrWhiteSpace(command) ? "" : $"& {{ {command} }}";

            runner.Start("wt", $"-d \"{path}\" powershell -NoExit {hasCommand}", path);
        }
    }
}

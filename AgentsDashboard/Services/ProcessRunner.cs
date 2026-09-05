using System.Diagnostics;
using System.Text;
using AgentsDashboard.Models;

namespace AgentsDashboard.Services
{
    public class ProcessRunner
    {
        public async Task<ProcessResult> Run(string fileName, string arguments, string workingDir, Action<string>? onLine = null)
        {
            var info = new ProcessStartInfo
            {
                FileName = fileName,
                Arguments = arguments,
                WorkingDirectory = workingDir,
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                StandardOutputEncoding = Encoding.UTF8,
                StandardErrorEncoding = Encoding.UTF8
            };

            using var process = new Process { StartInfo = info };
            var output = new StringBuilder();

            void Append(string? line)
            {
                if (line == null)
                    return;
                output.AppendLine(line);
                onLine?.Invoke(line);
            }

            process.OutputDataReceived += (s, e) => Append(e.Data);
            process.ErrorDataReceived += (s, e) => Append(e.Data);

            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            await process.WaitForExitAsync();

            return new ProcessResult { ExitCode = process.ExitCode, Output = output.ToString().Trim() };
        }

        public void Start(string fileName, string arguments, string workingDir)
        {
            var info = new ProcessStartInfo
            {
                FileName = fileName,
                Arguments = arguments,
                WorkingDirectory = workingDir,
                UseShellExecute = true
            };

            Process.Start(info);
        }

        public void StartHidden(string fileName, string arguments, string workingDir)
        {
            var info = new ProcessStartInfo
            {
                FileName = fileName,
                Arguments = arguments,
                WorkingDirectory = workingDir,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            Process.Start(info);
        }
    }
}

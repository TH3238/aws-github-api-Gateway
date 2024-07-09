using System.Diagnostics;
using System.Threading.Tasks;

namespace awsGateway.Services
{
    public class GitHubCliService
    {
        public async Task<string> RunGitHubCliCommand(string command)
        {
            using var process = new Process();
            process.StartInfo = new ProcessStartInfo
            {
                FileName = "gh",
                Arguments = command,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            process.Start();
            string output = await process.StandardOutput.ReadToEndAsync();
            string error = await process.StandardError.ReadToEndAsync();
            await process.WaitForExitAsync();

            return process.ExitCode == 0 ? output : $"Error: {error}";
        }
    }
}
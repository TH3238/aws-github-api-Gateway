using System.Diagnostics;

namespace awsGateway.Services
{
    public class AwsService : IAwsService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<AwsService> _logger;
        private readonly string _awsCliPath;

        public AwsService(IConfiguration configuration, ILogger<AwsService> logger)
        {
            _configuration = configuration;
            _logger = logger;
            _awsCliPath = configuration["AWS:CliPath"] ?? "aws";
        }

        public async Task<(bool Success, string Output)> RunCommandAsync(string command)
        {
            try
            {
                using var process = new Process();
                process.StartInfo = new ProcessStartInfo
                {
                    FileName = _awsCliPath,
                    Arguments = command,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };
                process.StartInfo.EnvironmentVariables["AWS_DEFAULT_REGION"] = _configuration["AWS:Region"];

                _logger.LogInformation($"Running AWS command: {command}");
                process.Start();

                string output = await process.StandardOutput.ReadToEndAsync();
                string error = await process.StandardError.ReadToEndAsync();

                await process.WaitForExitAsync();

                if (process.ExitCode == 0)
                {
                    _logger.LogInformation($"AWS command executed successfully: {output}");
                    return (true, output);
                }
                else
                {
                    _logger.LogError($"AWS command failed: {error}");
                    return (false, error);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error executing AWS command: {ex.Message}");
                return (false, $"Error: {ex.Message}");
            }
        }
    }
}
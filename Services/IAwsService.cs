namespace awsGateway.Services
{
    public interface IAwsService
    {
        Task<(bool Success, string Output)> RunCommandAsync(string command);
    }
}
using awsGateway.Services;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text;

public class GitHubService : IGitHubService
{
    private readonly HttpClient _httpClient;
    private readonly string _githubToken;
    private readonly ILogger<GitHubService> _logger;

    public GitHubService(HttpClient httpClient, IConfiguration configuration, ILogger<GitHubService> logger)
    {
        _httpClient = httpClient;
        _githubToken = configuration["GitHub:Token"];
        _logger = logger;

        if (string.IsNullOrEmpty(_githubToken))
        {
            throw new InvalidOperationException("GitHub token is not configured.");
        }

        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("token", _githubToken);
        _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github.v3+json"));
        _httpClient.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("YourApp", "1.0"));
    }

    public async Task<object> ManageUserRepository(string user, string repo, List<string> permissions)
    {
        try
        {
            var url = $"https://api.github.com/repos/{repo}/collaborators/{user}";

            var content = new StringContent(JsonSerializer.Serialize(new { permission = GetHighestPermission(permissions) }), Encoding.UTF8, "application/json");

            var response = await _httpClient.PutAsync(url, content);
            var responseContent = await response.Content.ReadAsStringAsync();

            _logger.LogInformation($"GitHub API Response: {response.StatusCode} - {responseContent}");

            return response.StatusCode switch
            {
                System.Net.HttpStatusCode.Created => new { status = "success", message = $"Collaborator {user} added to {repo} with permissions: {string.Join(", ", permissions)}" },
                System.Net.HttpStatusCode.NoContent => new { status = "info", message = $"{user} is already a collaborator in {repo}. Permissions updated to: {string.Join(", ", permissions)}" },
                System.Net.HttpStatusCode.NotFound => new { status = "error", message = $"Repository {repo} not found" },
                System.Net.HttpStatusCode.Forbidden => new { status = "error", message = $"Permission denied: {GetMessageFromContent(responseContent)}" },
                System.Net.HttpStatusCode.UnprocessableEntity => new { status = "error", message = $"Validation failed: {GetMessageFromContent(responseContent)}" },
                _ => new { status = "error", message = $"Failed to add collaborator: {GetMessageFromContent(responseContent) ?? "Unknown error"}" }
            };
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error in GrantGitHubAccess");
            return new { status = "error", message = $"An error occurred: {e.Message}" };
        }
    }

    public async Task<object> RemoveUserFromRepo(string user, string repo)
    {
        try
        {
            // First, check if the repo exists and the user is a collaborator
            var checkUrl = $"https://api.github.com/repos/{repo}/collaborators/{user}";
            var checkResponse = await _httpClient.GetAsync(checkUrl);

            if (checkResponse.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return new { status = "error", message = $"Repository {repo} not found or user {user} is not a collaborator." };
            }

            // If the check passes, proceed to remove the user
            var removeUrl = $"https://api.github.com/repos/{repo}/collaborators/{user}";
            var response = await _httpClient.DeleteAsync(removeUrl);
            var responseContent = await response.Content.ReadAsStringAsync();

            _logger.LogInformation($"GitHub API Response: {response.StatusCode} - {responseContent}");

            return response.StatusCode switch
            {
                System.Net.HttpStatusCode.NoContent => new { status = "success", message = $"User {user} removed from {repo}" },
                System.Net.HttpStatusCode.NotFound => new { status = "error", message = $"Repository {repo} or user {user} not found" },
                System.Net.HttpStatusCode.Forbidden => new { status = "error", message = $"Permission denied: {GetMessageFromContent(responseContent)}" },
                _ => new { status = "error", message = $"Failed to remove user: {GetMessageFromContent(responseContent) ?? "Unknown error"}" }
            };
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error in RemoveUserFromRepo");
            return new { status = "error", message = $"An error occurred: {e.Message}" };
        }
    }

    private string GetHighestPermission(List<string> permissions)
    {
        if (permissions.Contains("admin")) return "admin";
        if (permissions.Contains("maintain")) return "maintain";
        if (permissions.Contains("push")) return "push";
        if (permissions.Contains("triage")) return "triage";
        return "pull";
    }

    private string GetMessageFromContent(string content)
    {
        try
        {
            var jsonDocument = JsonDocument.Parse(content);
            return jsonDocument.RootElement.TryGetProperty("message", out var messageElement) ? messageElement.GetString() : null;
        }
        catch
        {
            return null;
        }
    }
}
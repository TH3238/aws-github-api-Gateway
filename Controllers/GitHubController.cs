using awsGateway.Services;
using Microsoft.AspNetCore.Mvc;
using awsGateway.Models;

[ApiController]
[Route("api/[controller]")]
public class GitHubController : ControllerBase
{
    private readonly IGitHubService _gitHubService;
    public GitHubController(IGitHubService gitHubService)
    {
        _gitHubService = gitHubService;
    }

    [HttpPost("ManageUser-repository")]
    public async Task<IActionResult> ManageUserRepository([FromBody] UserProjectRequest request)
    {
        var result = await _gitHubService.ManageUserRepository(request.Username, request.ProjectName, request.Permissions);
        return Ok(result);
    }

    [HttpDelete("remove-user")]
    public async Task<IActionResult> RemoveUser([FromBody] GitHubAccessRequest request)
    {
        var result = await _gitHubService.RemoveUserFromRepo(request.User, request.Repo);
        return Ok(result);
    }
}

public class GitHubAccessRequest
{
    public string User { get; set; }
    public string Repo { get; set; }
}
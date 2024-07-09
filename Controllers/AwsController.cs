using awsGateway.Services;
using awsGateway.Models;
using Microsoft.AspNetCore.Mvc;

namespace awsGateway.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AwsController : ControllerBase
    {
        private readonly IAwsService _awsService;

        public AwsController(IAwsService awsService)
        {
            _awsService = awsService;
        }

        [HttpGet("run-command")]
        public async Task<IActionResult> RunAwsCommand([FromQuery] string command)
        {
            if (string.IsNullOrWhiteSpace(command))
            {
                return BadRequest("Command cannot be empty");
            }
            var (success, output) = await _awsService.RunCommandAsync(command);
            return success ? Ok(output) : BadRequest(output);
        }

        [HttpGet("create-user")]
        public async Task<IActionResult> CreateUser([FromQuery] string username)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                return BadRequest("Username cannot be empty");
            }
            var command = $"iam create-user --user-name {username}";
            var (success, output) = await _awsService.RunCommandAsync(command);
            return success ? Ok($"User {username} created successfully") : BadRequest($"Failed to create user: {output}");
        }

        [HttpGet("create-project")]
        public async Task<IActionResult> CreateProject([FromQuery] string projectName)
        {
            if (string.IsNullOrWhiteSpace(projectName))
            {
                return BadRequest("Project name cannot be empty");
            }

            var createGroupCommand = $"iam create-group --group-name {projectName}";
            var (success, output) = await _awsService.RunCommandAsync(createGroupCommand);

            return success ? Ok($"Project (Group) '{projectName}' created successfully") : BadRequest($"Failed to create project: {output}");
        }

        [HttpPut("manage-user-project")]
        public async Task<IActionResult> ManageUserProject([FromQuery] string username, [FromQuery] string projectName, [FromQuery] ProjectPolicy policy)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(projectName))
            {
                return BadRequest("Username and ProjectName are required");
            }

           // 1.Create project(if not exists)
            var createGroupCommand = $"iam create-group --group-name {projectName}";
            var (createGroupSuccess, createGroupOutput) = await _awsService.RunCommandAsync(createGroupCommand);
            if (!createGroupSuccess && !createGroupOutput.Contains("EntityAlreadyExists"))
            {
                return BadRequest($"Failed to create project: {createGroupOutput}");
            }

            //2.Create user(if not exists)
            var createUserCommand = $"iam create-user --user-name {username}";
            var (createUserSuccess, createUserOutput) = await _awsService.RunCommandAsync(createUserCommand);
            if (!createUserSuccess && !createUserOutput.Contains("EntityAlreadyExists"))
            {
                return BadRequest($"Failed to create user: {createUserOutput}");
            }

            //3.Add user to project(if not already in)
            var addToGroupCommand = $"iam add-user-to-group --user-name {username} --group-name {projectName}";
            await _awsService.RunCommandAsync(addToGroupCommand);

            //4.Update policy
           var listPoliciesCommand = $"iam list-attached-group-policies --group-name {projectName}";
            var (listSuccess, listOutput) = await _awsService.RunCommandAsync(listPoliciesCommand);
            if (!listSuccess)
            {
                return BadRequest($"Failed to list policies for project {projectName}: {listOutput}");
            }

            //5.Detach all existing policies
            foreach (var existingPolicy in Enum.GetValues(typeof(ProjectPolicy)))
            {
                string policyArn = $"arn:aws:iam::aws:policy/{existingPolicy}";
                var detachPolicyCommand = $"iam detach-group-policy --group-name {projectName} --policy-arn {policyArn}";
                await _awsService.RunCommandAsync(detachPolicyCommand);
            }

            //6.Attach the new policy
            string newPolicyArn = $"arn:aws:iam::aws:policy/{policy}";
            var attachPolicyCommand = $"iam attach-group-policy --group-name {projectName} --policy-arn {newPolicyArn}";
            var (attachSuccess, attachOutput) = await _awsService.RunCommandAsync(attachPolicyCommand);
            if (!attachSuccess)
            {
                return BadRequest($"Failed to attach new policy {policy}: {attachOutput}");
            }

            return Ok($"User {username} managed in project {projectName} with policy {policy}");
        }

        [HttpDelete("remove-user-from-project")]
        public async Task<IActionResult> RemoveUserFromProject([FromQuery] string username, [FromQuery] string projectName)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(projectName))
            {
                return BadRequest("Username and ProjectName are required");
            }

            //1.Check if the user exists
            var checkUserCommand = $"iam get-user --user-name {username}";
            var (userExists, userOutput) = await _awsService.RunCommandAsync(checkUserCommand);
            if (!userExists)
            {
                return BadRequest($"User {username} does not exist");
            }

            //2.Check if the project(group) exists
           var checkGroupCommand = $"iam get-group --group-name {projectName}";
            var (groupExists, groupOutput) = await _awsService.RunCommandAsync(checkGroupCommand);
            if (!groupExists)
            {
                return BadRequest($"Project (group) {projectName} does not exist");
            }

            //3.Remove user from the project(group)
            var removeFromGroupCommand = $"iam remove-user-from-group --user-name {username} --group-name {projectName}";
            var (removeSuccess, removeOutput) = await _awsService.RunCommandAsync(removeFromGroupCommand);
            if (!removeSuccess)
            {
                return BadRequest($"Failed to remove user from project: {removeOutput}");
            }

            return Ok($"User {username} removed from project {projectName}");
        }
    }
}
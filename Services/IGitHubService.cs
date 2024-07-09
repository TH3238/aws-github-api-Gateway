using System.Threading.Tasks;
using System.Collections.Generic;

namespace awsGateway.Services
{
    public interface IGitHubService
    {
        Task<object> ManageUserRepository(string user, string repo, List<string> permissions);
        Task<object> RemoveUserFromRepo(string user, string repo);
    }
}
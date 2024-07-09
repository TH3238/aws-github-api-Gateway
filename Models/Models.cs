namespace awsGateway.Models
{
    public class CreateUserRequest
    {
        public string Username { get; set; }
    }

    public class CreateProjectRequest
    {
        public string ProjectName { get; set; }
    }

    public class UserProjectRequest
    {
        public string Username { get; set; }
        public string ProjectName { get; set; }
        public List<string> Permissions { get; set; }
    }

    public enum ProjectPolicy
    {
        AdministratorAccess,
        PowerUserAccess,
        ReadOnlyAccess
    }
}
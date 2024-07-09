# AWS and GitHub API Gateway

This project provides an API gateway for managing AWS and GitHub resources. It allows users to perform various operations such as creating users, managing projects, and handling GitHub repository collaborations.

## Prerequisites

Before you begin, ensure you have the following installed:

- [.NET 6.0 SDK](https://dotnet.microsoft.com/download/dotnet/6.0) or later
- [AWS CLI](https://aws.amazon.com/cli/)
- [GitHub CLI](https://cli.github.com/)

## Configuration

1. AWS CLI Setup:
   - Install the AWS CLI by following the [official AWS CLI installation guide](https://docs.aws.amazon.com/cli/latest/userguide/cli-configure-quickstart.html).
   - Configure your AWS credentials using `aws configure` command.

2. GitHub CLI Setup:
   - Install the GitHub CLI by following the [official GitHub CLI installation guide](https://cli.github.com/manual/installation).
   - Authenticate with GitHub using `gh auth login` command.

3. Update `appsettings.json`:
   - Set the correct AWS region in the `AWS:Region` field.
   - Update the `AWS:CliPath` to point to your AWS CLI executable (if different from the default).
   - Add your GitHub personal access token in the `GitHub:Token` field.

## Installation

1. Clone the repository:
    git clone https://github.com/TH3238/aws-github-api-Gateway
   cd aws-github-api-gateway
2. Restore dependencies:
    dotnet restore
3. Build the project:
    dotnet build

## Running the Application

To run the application, use the following command:
dotnet run
Copy
The API will be available at `https://localhost:5001` by default.

## API Endpoints

  ### AWS Controller
  
  - `GET /api/Aws/run-command`: Run an AWS CLI command
  - `GET /api/Aws/create-user`: Create an AWS IAM user
  - `GET /api/Aws/create-project`: Create an AWS IAM group (project)
  - `PUT /api/Aws/manage-user-project`: Manage user's project membership and permissions
  - `DELETE /api/Aws/remove-user-from-project`: Remove a user from a project
  
  ### GitHub Controller
  
  - `POST /api/GitHub/ManageUser-repository`: Manage user's repository access
  - `DELETE /api/GitHub/remove-user`: Remove a user from a repository

## Security Considerations

- Ensure that your AWS credentials and GitHub token are kept secure and not shared publicly.
- Use appropriate IAM policies to restrict access to AWS resources.
- Regularly rotate your GitHub personal access token.

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

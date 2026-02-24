using migration_accelerator.Models;

namespace migration_accelerator.Services
{
    public interface IGitHubService
    {
        Task<string> CreateRepositoryAsync(GitHubCreateRepoRequest request);
        Task UploadFileAsync(string repoName, string fileName, byte[] fileBytes);

        Task<List<string>> GetRepositoriesAsync();
        Task<List<string>> GetBranchesAsync(string repoName);
        Task CreateBranchAsync(string repoName, string newBranch, string sourceBranch);


    }

}

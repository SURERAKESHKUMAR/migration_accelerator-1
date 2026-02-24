using migration_accelerator.Models;
using migration_accelerator.Services;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

public class GitHubService : IGitHubService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _config;

    public GitHubService(HttpClient httpClient, IConfiguration config)
    {
        _httpClient = httpClient;
        _config = config;
    }

    public async Task<string> CreateRepositoryAsync(GitHubCreateRepoRequest request)
    {
        var token = _config["GitHub:Token"];

        _httpClient.DefaultRequestHeaders.Clear();
        _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("token", token);

        _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("ASP.NET Core App");

        var json = JsonSerializer.Serialize(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync("user/repos", content);
        var result = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
            throw new Exception($"GitHub Error: {result}");

        return result;
    }

    public async Task UploadFileAsync(string repoName, string fileName, byte[] fileBytes)
    {
        var token = _config["GitHub:Token"];
        var owner = await GetGitHubUsername(); // dynamic owner

        var base64Content = Convert.ToBase64String(fileBytes);

        var body = new
        {
            message = $"Adding {fileName} via automation",
            content = base64Content,
            branch = "main"
        };

        var json = JsonSerializer.Serialize(body);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        _httpClient.DefaultRequestHeaders.Clear();
        _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("token", token);
        _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("ASP.NET Core App");

        var url = $"repos/{owner}/{repoName}/contents/{fileName}";
        var response = await _httpClient.PutAsync(url, content);

        var result = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
            throw new Exception($"File Upload Failed: {result}");
    }
    private async Task<string> GetGitHubUsername()
    {
        var response = await _httpClient.GetAsync("user");
        var json = await response.Content.ReadAsStringAsync();

        using var doc = JsonDocument.Parse(json);
        return doc.RootElement.GetProperty("login").GetString();
    }

    private void SetHeaders()
    {
        var token = _config["GitHub:Token"];

        _httpClient.DefaultRequestHeaders.Clear();
        _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("token", token);
        _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("ASP.NET Core App");
    }
    private async Task<string> GetOwnerAsync()
    {
        SetHeaders();
        var res = await _httpClient.GetAsync("user");
        var json = await res.Content.ReadAsStringAsync();

        using var doc = JsonDocument.Parse(json);
        return doc.RootElement.GetProperty("login").GetString();
    }

    public async Task<List<string>> GetRepositoriesAsync()
    {
        SetHeaders();

        var repos = new List<string>();
        var res = await _httpClient.GetAsync("user/repos");
        var json = await res.Content.ReadAsStringAsync();

        using var doc = JsonDocument.Parse(json);

        foreach (var repo in doc.RootElement.EnumerateArray())
            repos.Add(repo.GetProperty("name").GetString());

        return repos;
    }

    public async Task<List<string>> GetBranchesAsync(string repoName)
    {
        SetHeaders();
        var owner = await GetOwnerAsync();

        var branches = new List<string>();
        var res = await _httpClient.GetAsync($"repos/{owner}/{repoName}/branches");
        var json = await res.Content.ReadAsStringAsync();

        using var doc = JsonDocument.Parse(json);

        foreach (var b in doc.RootElement.EnumerateArray())
            branches.Add(b.GetProperty("name").GetString());

        return branches;
    }

    private async Task<string> GetBranchShaAsync(string repoName, string branch)
    {
        SetHeaders();
        var owner = await GetOwnerAsync();

        var res = await _httpClient.GetAsync($"repos/{owner}/{repoName}/git/ref/heads/{branch}");
        var json = await res.Content.ReadAsStringAsync();

        using var doc = JsonDocument.Parse(json);

        return doc.RootElement
                  .GetProperty("object")
                  .GetProperty("sha")
                  .GetString();
    }

    public async Task CreateBranchAsync(string repoName, string newBranch, string sourceBranch)
    {
        var sha = await GetBranchShaAsync(repoName, sourceBranch);
        SetHeaders();
        var owner = await GetOwnerAsync();

        var body = new
        {
            @ref = $"refs/heads/{newBranch}",
            sha = sha
        };

        var content = new StringContent(
            JsonSerializer.Serialize(body),
            Encoding.UTF8,
            "application/json");

        var res = await _httpClient.PostAsync($"repos/{owner}/{repoName}/git/refs", content);
        var result = await res.Content.ReadAsStringAsync();

        if (!res.IsSuccessStatusCode)
            throw new Exception(result);
    }
}

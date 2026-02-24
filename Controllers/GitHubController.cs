using Microsoft.AspNetCore.Mvc;
using migration_accelerator.Models;
using migration_accelerator.Services;
using System.IO.Compression;

public class GitHubController : Controller
{
    private readonly IGitHubService _gitHubService;

    public GitHubController(IGitHubService gitHubService)
    {
        _gitHubService = gitHubService;
    }

    public IActionResult Create()
    {
        return View();
    }
    [HttpPost]
    public async Task<IActionResult> Create(CreateRepoViewModel model)
    {
        try
        {
            var request = new GitHubCreateRepoRequest
            {
                name = model.RepoName,
                description = model.Description,
                @private = model.IsPrivate,
                auto_init = true,
                default_branch = "main"
            };

            await _gitHubService.CreateRepositoryAsync(request);

            // ✅ OPTIONAL FILE UPLOAD
            if (model.UploadFile != null && model.UploadFile.Length > 0)
            {
                using var ms = new MemoryStream();
                await model.UploadFile.CopyToAsync(ms);

                // Check if zip
                if (Path.GetExtension(model.UploadFile.FileName).Equals(".zip", StringComparison.OrdinalIgnoreCase))
                {
                    ms.Position = 0;

                    using var archive = new ZipArchive(ms, ZipArchiveMode.Read);

                    foreach (var entry in archive.Entries)
                    {
                        // Skip folders
                        if (string.IsNullOrEmpty(entry.Name))
                            continue;

                        using var entryStream = entry.Open();
                        using var entryMs = new MemoryStream();
                        await entryStream.CopyToAsync(entryMs);

                        var fileBytes = entryMs.ToArray();

                        // IMPORTANT: keep folder structure
                        var filePathInRepo = entry.FullName.Replace("\\", "/");

                        await _gitHubService.UploadFileAsync(
                            model.RepoName,
                            filePathInRepo,
                            fileBytes);
                    }
                }
                else
                {
                    // Normal single file upload
                    await _gitHubService.UploadFileAsync(
                        model.RepoName,
                        model.UploadFile.FileName,
                        ms.ToArray());
                }
            }


            TempData["Success"] = "Repository created and file uploaded (if provided)!";
            return RedirectToAction("Create");
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
            return View(model);
        }
    }


    public async Task<IActionResult> CreateBranch()
    {
        var model = new CreateBranchViewModel
        {
            Repositories = await _gitHubService.GetRepositoriesAsync()
        };

        return View(model);
    }

    [HttpGet]
    public async Task<JsonResult> GetBranches(string repoName)
    {
        var branches = await _gitHubService.GetBranchesAsync(repoName);
        return Json(branches);
    }

    [HttpPost]
    public async Task<IActionResult> CreateBranch(CreateBranchViewModel model)
    {
        try
        {
            await _gitHubService.CreateBranchAsync(
                model.RepoName,
                model.NewBranchName,
                model.SourceBranch);

            TempData["Success"] = "Feature branch created!";
            return RedirectToAction("CreateBranch");
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
            model.Repositories = await _gitHubService.GetRepositoriesAsync();
            return View(model);
        }
    }
}

using Microsoft.AspNetCore.Mvc;
using migration_accelerator.Models;
using migration_accelerator.Services;
using Newtonsoft.Json.Linq;
using System;

namespace migration_accelerator.Controllers
{
    public class ScanController : Controller
    {
        private readonly ScanService _scanService;
        private readonly string _scanFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "scan-results");

        public ScanController(ScanService scanService)
        {
            _scanService = scanService;
        }

        public IActionResult Index(string selectedRepo = null)
        {
            var viewModel = new SecretsViewModel();

            if (Directory.Exists(_scanFolder))
            {
                var jsonFiles = Directory.GetFiles(_scanFolder, "*.json");

                // Populate dropdown
                viewModel.AllRepos = jsonFiles
                    .Select(f =>
                    {
                        var j = JObject.Parse(System.IO.File.ReadAllText(f));
                        return j["repoName"]?.ToString() ?? Path.GetFileNameWithoutExtension(f);
                    })
                    .ToList();

                if (!string.IsNullOrEmpty(selectedRepo))
                {
                    var filePath = jsonFiles.FirstOrDefault(f =>
                    {
                        var j = JObject.Parse(System.IO.File.ReadAllText(f));
                        return (j["repoName"]?.ToString() ?? "").Equals(selectedRepo, System.StringComparison.OrdinalIgnoreCase);
                    });

                    if (filePath != null)
                    {
                        var jsonData = System.IO.File.ReadAllText(filePath);
                        var jsonObj = JObject.Parse(jsonData);

                        viewModel.RepoName = jsonObj["repoName"]?.ToString() ?? "Unknown";
                        viewModel.ScanDate = jsonObj["scanDate"]?.ToString() ?? "";

                        viewModel.Matches = jsonObj["findings"]?["value"]?
                            .Select(f =>
                            {
                                var full = f["Match"]?.ToString() ?? "";

                                // Handle multi-line secrets (RSA keys etc.)
                                if (full.Contains("-----BEGIN") && full.Contains("-----END"))
                                {
                                    return new SecretKeyValue
                                    {
                                        Key = full.Split(new string[] { "-----BEGIN" }, System.StringSplitOptions.None)[0].Trim(),
                                        Value = full.Trim()
                                    };
                                }

                                // Split at first '=' for normal secrets
                                var parts = full.Split(new char[] { '=', ':' }, StringSplitOptions.None);
                                if (parts.Length > 2)
                                {
                                    parts = new string[] { parts[0], string.Join("=", parts.Skip(1)) };
                                }

                                var key = parts[0].Trim();
                                var value = parts.Length > 1 ? parts[1].Trim() : "";
                                return new SecretKeyValue
                                {
                                    Key = string.IsNullOrEmpty(key) ? full : key,
                                    Value = value
                                };
                            })
                            .Where(m => !string.IsNullOrEmpty(m.Key))
                            .ToList() ?? new List<SecretKeyValue>();
                    }
                }
            }

            return View(viewModel);
        }


        [HttpPost]
        public async Task<IActionResult> ScanSecrets(string folderPath, string repoName)
        {
            try
            {
                // Scan the repo
                var results = await _scanService.ScanRepo(folderPath, repoName); // returns List<SecretKeyValue>

                var viewModel = new SecretsViewModel
                {
                    RepoName = repoName,
                    ScanDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    Matches = results // directly assign list of SecretKeyValue
                };

                if (!viewModel.Matches.Any())
                    ViewBag.Error = "Scan completed.";

                return View("Index", viewModel);
            }
            catch (Exception ex)
            {
                ViewBag.Error = ex.Message;
                return View("Index", new SecretsViewModel
                {
                    RepoName = repoName,
                    ScanDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    Matches = new List<SecretKeyValue>()
                });
            }
        }

    }

}

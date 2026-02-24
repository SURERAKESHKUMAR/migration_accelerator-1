using Microsoft.AspNetCore.Mvc;
using migration_accelerator.Models;
using System.Text.Json;

namespace migration_accelerator.Controllers
{
    public class MigrationReportController : Controller
    {
        private readonly IWebHostEnvironment _env;

        public MigrationReportController(IWebHostEnvironment env)
        {
            _env = env;
        }

        public IActionResult Index()
        {
            var path = Path.Combine(_env.WebRootPath, "data", "migrations_report.json");
            var json = System.IO.File.ReadAllText(path);
            dynamic data = Newtonsoft.Json.JsonConvert.DeserializeObject(json);
            return View(data.repositories);
        }

        public IActionResult ViewReport(string repoName)
        {
            var path = Path.Combine(
                _env.WebRootPath,
                "scan-results",
                $"{repoName}.json"
            );

            if (!System.IO.File.Exists(path))
                return NotFound($"Report not found for {repoName}");

            var json = System.IO.File.ReadAllText(path);

            var model = JsonSerializer.Deserialize<ScanReportViewModel>(
                json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
            );

            return View(model);
        }

        public IActionResult MigrationReport()
        {
            var jsonPath = Path.Combine(
                Directory.GetCurrentDirectory(),
                "wwwroot/data/migrations_report.json");

            var json = System.IO.File.ReadAllText(jsonPath);

            var model = JsonSerializer.Deserialize<MigrationReportViewModel>(json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            return View(model);
        }
    }
}

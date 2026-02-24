using System.Diagnostics;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using migration_accelerator.Models;
using migration_accelerator.Services;

namespace migration_accelerator.Controllers
{
    public class SecretsController : Controller
    {
        private readonly AwsSecretsService _aws;
        private readonly TerraformBackgroundService _bgService;
        private readonly TerraformExecutor _executor;

        public SecretsController(AwsSecretsService aws, TerraformBackgroundService bgService, TerraformExecutor executor)
        {
            _aws = aws;
            _bgService = bgService;
            _executor = executor;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View(new SecretsViewModel());
        }
        [HttpPost]
        public IActionResult LoadFile(IFormFile secretsFile, string environment, string secretName)
        {
            if (secretsFile == null || secretsFile.Length == 0)
            {
                ModelState.AddModelError("", "Please upload a valid TXT file");
                return View("Index", new SecretsViewModel());
            }

            var secrets = TxtSecretReader.ReadFromStream(secretsFile.OpenReadStream());

            return View("Index", new SecretsViewModel
            {
                Environment = environment,
                SecretName = secretName,
                Secrets = secrets
            });
        }


        [HttpPost]
        public async Task<IActionResult> Migrate(SecretsViewModel model)
        {
            var secretName = $"{model.SecretName}/{model.FileName}";

            await _aws.SaveAsync(secretName, model.Secrets);

            TempData["ToastMessage"] = "Secrets migrated successfully to AWS!";
            TempData["ToastType"] = "success";
            return RedirectToAction("Index");

        }

        [HttpGet]
        public async Task<IActionResult> PreviewSecretesIndex(string secretName)
        {
            var model = new SecretsPreviewViewModel();

            // 🔹 Load dropdown secrets
            model.AvailableSecrets = await _aws.GetAllSecretNamesAsync();
            model.SecretName = secretName ?? string.Empty;

            if (string.IsNullOrWhiteSpace(secretName))
                return View(model);

            try
            {
                model.Secrets = await _aws.GetSecretsAsync(secretName);

                // ✅ SUCCESS TOAST
                TempData["ToastMessage"] = "Secrets loaded successfully!";
                TempData["ToastType"] = "success";
            }
            catch (Exception)
            {

                TempData["ToastMessage"] = $"Secrets not found for: {secretName}";
                TempData["ToastType"] = "error";

                model.ErrorMessage = "Secrets are not found or inaccessible.";
            }

            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> MigrateToAws([FromBody] AwsSecretRequest request)
        {
            if (request == null || request.Secrets == null || !request.Secrets.Any())
            {
                return Json(new { success = false, message = "No secrets received from UI!" });
            }

            // remove duplicate keys
            var cleanDict = request.Secrets
                .GroupBy(x => x.Key)
                .ToDictionary(g => g.Key, g => g.Last().Value);

            var finalSecretName = request.SecretName.Trim();

            await _aws.SaveAsync(finalSecretName,
                cleanDict.Select(x => new SecretItem
                {
                    Key = x.Key,
                    Value = x.Value
                }).ToList());

            return Json(new
            {
                success = true,
                message = $"Secrets stored in AWS → {finalSecretName}"
            });
        }


    }
}


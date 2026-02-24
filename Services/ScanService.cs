using migration_accelerator.Models;
using System.Diagnostics;
using System.Text.Json;

namespace migration_accelerator.Services
{
    public class ScanService
    {
        private readonly string _batFile = @"C:\Tools\scan-repo.bat";
        private readonly string _resultFolder = @"C:\Users\JadiRajKumar\OneDrive - ValueMomentum, Inc\Desktop\TFS_TO_GITGUB_MIGRATION_2026\migration_accelerator-master\migration_accelerator-master\migration_accelerator\wwwroot\scan-results";

        public async Task<List<SecretKeyValue>> ScanRepo(string folderPath, string repoName)
        {
            var repoFullPath = Path.Combine(folderPath, repoName);

            if (!Directory.Exists(repoFullPath))
                throw new Exception("Repository not found in selected path!");

            var psi = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = $"/c \"cd /d \"{repoFullPath}\" && \"{_batFile}\"\"",
                RedirectStandardInput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = Process.Start(psi);

            if (process == null)
                throw new Exception("Failed to start scan process.");

            await process.StandardInput.WriteLineAsync(repoName);
            process.StandardInput.Close();
            await process.WaitForExitAsync();

            var jsonPath = Path.Combine(_resultFolder, $"{repoName}.json");

            if (!File.Exists(jsonPath))
                throw new Exception("Scan result JSON not found!");

            var json = await File.ReadAllTextAsync(jsonPath);

            var list = new List<SecretKeyValue>();
            var doc = JsonDocument.Parse(json);

            // ✅ Handle BOTH JSON structures
            JsonElement findingsArray;

            if (doc.RootElement.ValueKind == JsonValueKind.Array)
            {
                findingsArray = doc.RootElement;
            }
            else if (doc.RootElement.TryGetProperty("Leaks", out var leaks))
            {
                findingsArray = leaks;
            }
            else
            {
                return list;
            }

            foreach (var item in findingsArray.EnumerateArray())
            {
                if (item.TryGetProperty("Match", out var matchProp))
                {
                    var text = matchProp.GetString();

                    if (string.IsNullOrWhiteSpace(text))
                        continue;

                    text = text.Replace("\r", "").Trim();

                    var parts = text.Split('=', 2);

                    if (parts.Length == 2)
                    {
                        list.Add(new SecretKeyValue
                        {
                            Key = parts[0].Trim(),
                            Value = parts[1].Trim()
                        });
                    }
                }
            }

            return list; // ✅ IMPORTANT
        }
    }
}

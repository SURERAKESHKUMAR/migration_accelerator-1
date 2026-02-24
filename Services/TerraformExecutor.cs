using System.Diagnostics;
using System.Text.Json;

namespace migration_accelerator.Services;

public class TerraformExecutor
{
    public async Task RunAsync(string env, string secretName, Dictionary<string, string> secrets)
    {
        var terraformPath = Path.Combine(Directory.GetCurrentDirectory(), "terraform");
        Directory.CreateDirectory(terraformPath);

        var tfvarsPath = Path.Combine(terraformPath, "secrets.auto.tfvars.json");

        var terraformObject = new
        {
            environment = env,
            secret_name = secretName,
            secrets
        };

        await File.WriteAllTextAsync(
            tfvarsPath,
            JsonSerializer.Serialize(terraformObject, new JsonSerializerOptions
            {
                WriteIndented = true
            }));

        var psi = new ProcessStartInfo
        {
            FileName = "cmd.exe",
            Arguments = "/c terraform init && terraform apply -auto-approve",
            WorkingDirectory = terraformPath,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = Process.Start(psi);
        await process!.WaitForExitAsync();
    }
}

using System.Diagnostics;
using System.Text.Json;
using System.Threading.Channels;

namespace migration_accelerator.Services;

public class TerraformBackgroundService : BackgroundService
{
    private readonly Channel<Func<Task>> _queue = Channel.CreateUnbounded<Func<Task>>();

    public void QueueTask(Func<Task> task)
    {
        _queue.Writer.TryWrite(task);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var task in _queue.Reader.ReadAllAsync(stoppingToken))
        {
            try
            {
                await task();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Terraform error: {ex.Message}");
            }
        }
    }
    public static async Task RunTerraformAsync(string env, string secretName, Dictionary<string, string> secrets)
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

    await File.WriteAllTextAsync(tfvarsPath,
        JsonSerializer.Serialize(terraformObject, new JsonSerializerOptions { WriteIndented = true }));

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

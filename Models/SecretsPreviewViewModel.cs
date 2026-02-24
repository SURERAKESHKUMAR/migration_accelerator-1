namespace migration_accelerator.Models
{
    public class SecretsPreviewViewModel
    {
        public string SecretName { get; set; } = "";
        public Dictionary<string, string> Secrets { get; set; } = new();
        public List<string> AvailableSecrets { get; set; } = new();
        public string? ErrorMessage { get; set; }
    }
}

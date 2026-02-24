using System.Collections.Generic;

namespace migration_accelerator.Models
{
    public class AwsSecretRequest
    {
        public string Environment { get; set; } = string.Empty;
        public string SecretName { get; set; } = string.Empty;
        public List<SecretItem> Secrets { get; set; } = new();
    }

   
}

using System.ComponentModel.DataAnnotations;

namespace migration_accelerator.Models
{
    public class SecretsViewModel
    {

        [Required]
        [Display(Name = "AWS Secret Name")]
        public string SecretName { get; set; }
        public string FileName { get; set; }
        public string Environment { get; set; }
        public List<SecretItem> Secrets { get; set; } = new List<SecretItem>();

        public string RepoName { get; set; } = "No Data";

        public string ScanDate { get; set; }
        public List<SecretKeyValue> Matches { get; set; } = new List<SecretKeyValue>();
        public List<string> AllRepos { get; set; } = new List<string>();
         // Secrets coming from UI table
  

    }
}


using System.ComponentModel.DataAnnotations;

namespace migration_accelerator.Models
{
    public class RepositoryViewModel
    {
        [Required(ErrorMessage = "Repository name is required")]
        [Display(Name = "Repo Name")]
        public string RepoName { get; set; }

        [Display(Name = "Branches")]
        public List<string> SelectedBranches { get; set; } = new List<string>();

        // Example list of branches (could be from database or API)
        public List<string> Branches { get; set; } = new List<string>
        {
            "main",
            "develop",
            "feature-1",
            "bugfix-1"
        };
    }
}

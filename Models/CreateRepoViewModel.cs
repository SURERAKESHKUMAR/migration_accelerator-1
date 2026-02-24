namespace migration_accelerator.Models
{
    public class CreateRepoViewModel
    {
        public string RepoName { get; set; }
        public string Description { get; set; }
        public bool IsPrivate { get; set; }

        // OPTIONAL FILE
        public IFormFile? UploadFile { get; set; }

    }

}

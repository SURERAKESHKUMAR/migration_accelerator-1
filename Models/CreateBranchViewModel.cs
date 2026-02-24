namespace migration_accelerator.Models
{
    public class CreateBranchViewModel
    {
        public string RepoName { get; set; }
        public string SourceBranch { get; set; }
        public string NewBranchName { get; set; }

        public List<string>? Repositories { get; set; }
    }

}

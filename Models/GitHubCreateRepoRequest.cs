namespace migration_accelerator.Models
{
    public class GitHubCreateRepoRequest
    {
        public string name { get; set; }
        public string description { get; set; }
        public bool @private { get; set; }
        public bool auto_init { get; set; }
        public string default_branch { get; set; }
    }
}

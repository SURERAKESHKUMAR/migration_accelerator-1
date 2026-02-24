namespace migration_accelerator.Models
{
    public class RepoAssessment
    {
        public int Sno { get; set; }
        public string RepoName { get; set; }
        public string repoSizeMB { get; set; }
        public int sensitiveDataOccurrences { get; set; }
        public string Complexity { get; set; }
        public string ReportUrl { get; set; }
    }
}

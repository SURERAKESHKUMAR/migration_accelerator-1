namespace migration_accelerator.Models
{
    public class ScanReportViewModel
    {
        public string RepoName { get; set; }
        public string ScanDate { get; set; }
        public Findings Findings { get; set; }
        public List<string> Matches { get; set; }
    }
    public class Findings
    {
        public List<FindingItem> Value { get; set; }
    }
    public class FindingItem
    {
        public string Match { get; set; }
        public string Secret { get; set; }
        public string File { get; set; }
        public int StartColumn { get; set; }
        public int EndColumn { get; set; }
    }
}

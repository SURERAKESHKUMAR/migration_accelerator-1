namespace migration_accelerator.Models
{
    public class MigrationReportViewModel
    {
        public string Title { get; set; }
        public List<RepoAssessment> Repositories { get; set; } = new();
    }
}

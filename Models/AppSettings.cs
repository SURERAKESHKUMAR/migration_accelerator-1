namespace migration_accelerator.Models
{
    public class AppSettings
{
    public string OnBaseURL { get; set; }
    public string GC_OnBaseURL { get; set; }
    public bool ShowXMLs { get; set; }
    public bool ShowSimulator { get; set; }
    public bool LogDocumentsInDB { get; set; }
    public string ConnectionString { get; set; }
}

}
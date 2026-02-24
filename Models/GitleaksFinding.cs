using Newtonsoft.Json;

namespace migration_accelerator.Models
{
    public class GitleaksFinding
    {
        [JsonProperty("File")]
        public string File { get; set; }

        [JsonProperty("RuleID")]
        public string RuleID { get; set; }

        [JsonProperty("Secret")]
        public string Secret { get; set; }

        [JsonProperty("StartLine")]
        public int StartLine { get; set; }
    }
}

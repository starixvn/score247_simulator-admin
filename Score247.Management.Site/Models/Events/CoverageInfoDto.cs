using Newtonsoft.Json;

namespace Score247.Management.Site.Models.Events
{
    public class CoverageDto
    {
        public bool basic_score { get; set; }
        public bool key_events { get; set; }
        public bool detailed_events { get; set; }
        public bool lineups { get; set; }
        public bool commentary { get; set; }

        public CoverageDto()
        {
            basic_score = true;
            commentary = true;
            detailed_events = true;
            key_events = true;
            lineups = true;
        }
    }

    public class CoverageInfoDto
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string level { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public bool live_coverage { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public CoverageDto coverage { get; set; }

        public CoverageInfoDto()
        {
            level = "bronze";
            live_coverage = true;
            coverage = new CoverageDto();
        }
    }
}
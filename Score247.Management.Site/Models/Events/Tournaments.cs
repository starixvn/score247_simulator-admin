using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Score247.Management.Site.Models.Events
{
    public class Tournaments
    {
        public DateTime generated_at { get; set; }
        public string schema { get; set; }
        public IEnumerable<Tournament> tournaments { get; set; }
    }

    public class Tournament
    {
        public string id { get; set; }
        public string name { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public Sport sport { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public Category category { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public Season current_season { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public Season_Coverage_Info season_coverage_info { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public CoverageInfoDto coverage_info { get; set; }
    }

    public class Season_Coverage_Info
    {
        public string season_id { get; set; }
        public int scheduled { get; set; }
        public int played { get; set; }
        public string max_coverage_level { get; set; }
        public int max_covered { get; set; }
        public string min_coverage_level { get; set; }
    }
}
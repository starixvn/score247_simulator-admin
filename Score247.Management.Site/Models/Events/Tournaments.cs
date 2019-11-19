using System;
using System.Collections.Generic;

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
        public Sport sport { get; set; }
        public Category category { get; set; }
        public Season current_season { get; set; }
        public Season_Coverage_Info season_coverage_info { get; set; }
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
using System;
using System.Collections.Generic;
using Score247Management.Core.Leagues.Models;

namespace Score247Management.Core.Leagues.Dtos
{
    public class LeagueSportradarInfo
    {
        public DateTimeOffset generated_at { get; set; }
        public string schema { get; set; }

        public League tournament { get; set; }

        public LeagueSeason season { get; set; }

        public Round round { get; set; }
        public IEnumerable<Group> groups { get; set; }
        public SeasonCoverageInfo season_coverage_info { get; set; }
    }

    public class Round
    {
        public string type { get; set; }
        public int number { get; set; }
        public int cup_round_match_number { get; set; }
        public int cup_round_matches { get; set; }
    }

    public class Group
    {
        public string name { get; set; }
        public string id { get; set; }

        public IEnumerable<Team> teams { get; set; }
    }

    public class Team
    {
        public string id { get; set; }
        public string name { get; set; }
        public string country { get; set; }
        public string country_code { get; set; }
        public string abbreviation { get; set; }
    }

    public class SeasonCoverageInfo
    {
        public string season_id { get; set; }
        public int scheduled { get; set; }
        public int played { get; set; }
        public string max_coverage_level { get; set; }
        public int max_covered { get; set; }
        public string min_coverage_level { get; set; }
    }
}
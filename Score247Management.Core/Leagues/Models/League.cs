using Newtonsoft.Json;

namespace Score247Management.Core.Leagues.Models
{
    public class League
    {
        private string name;
        public string Id { get; set; }

        public string Name { get => name.Trim().TrimStart('*'); set => name = value; }

        public int Order { get; set; }

        public string Flag { get; set; }

        [JsonProperty("category")]
        public LeagueCategory LeagueCategory { get; set; }

        [JsonProperty("current_season")]
        public LeagueSeason LeagueSeason { get; set; }

        public bool IsActive { get; set; }
    }
}
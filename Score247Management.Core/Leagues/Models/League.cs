using Newtonsoft.Json;

namespace Score247Management.Core.Leagues.Models
{
    public class League
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public int Order { get; set; }

        public string Country { get; set; }
        public string CountryCode { get; set; }
        public string Region { get; set; }

        public string Flag
        {
            get
            {
                if (string.IsNullOrWhiteSpace(CountryCode))
                {
                    return "http://ha.nexdev.net:7209/Test/flags/default_flag.svg";
                }
                else
                {
                    return "http://ha.nexdev.net:7209/Test/flags/" + CountryCode + ".svg";
                }
            }
        }

        [JsonProperty("category")]
        public LeagueCategory LeagueCategory { get; set; }

        [JsonIgnore]
        public string CategoryId { get; set; }

        [JsonProperty("current_season")]
        public LeagueSeason LeagueSeason { get; set; }

        [JsonProperty("round")]
        public LeagueRound Round { get; set; }

        public bool IsActive { get; set; }
    }
}
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Score247Management.Core.Images
{
    public class RapidTeam
    {
        public RapidTeam(int teamId, string name, string logo, string code)
        {
            TeamId = teamId;
            Name = name;
            Code = code;
            Logo = logo;
        }

        [JsonProperty("team_id")]
        public int TeamId { get; set; }

        public string Name { get; set; }
        public string Code { get; set; }
        public string Logo { get; set; }
    }

    public class RapidTeamByLeague
    {
        public RapidTeamByLeagueApi Api { get; set; }
    }

    public class RapidTeamByLeagueApi
    {
        public int result { get; set; }
        public IEnumerable<RapidTeam> teams { get; set; }
    }
}
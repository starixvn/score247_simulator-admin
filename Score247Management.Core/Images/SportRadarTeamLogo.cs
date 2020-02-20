using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Score247Management.Core.Images
{
    public class SportRadarTeamLogo
    {
        public SportRadarTeamLogo(string teamId, string name, string code, string href)
        {
            TeamId = teamId;
            Name = name;
            Code = code;
            Href = href;
        }

        public string TeamId { get; set; }

        public string Name { get; set; }

        public string Code { get; set; }

        public string Href { get; set; }
    }

    public class SportRadarTeamLogoResponse
    {
        public IEnumerable<SportRadarTeamAsset> Assetlist { get; set; }
    }

    public class SportRadarTeamAsset
    {
        [JsonProperty("team_id")]
        public string TeamId { get; set; }

        public string Title { get; set; }

        public IEnumerable<SportRadarLink> Links { get; set; }
    }

    public class SportRadarLink
    {
        public string Type { get; set; }

        public string Href { get; set; }
    }
}
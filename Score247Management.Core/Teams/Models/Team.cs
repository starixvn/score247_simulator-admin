namespace Score247Management.Core.Teams.Models
{
    using System.Collections.Generic;
    using Newtonsoft.Json;
    using Score247Management.Core.Base;

    public class Team : BaseModel
    {
        public string Country { get; set; }

        public string CountryCode { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Flag { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public bool IsHome { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public IEnumerable<Player> Players { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public TeamStatistic Statistic { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public Coach Coach { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Formation { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Abbreviation { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public IEnumerable<Player> Substitutions { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string qualifier { get; set; }
    }
}
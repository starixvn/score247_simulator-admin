namespace Score247Management.Core.Teams.Models
{
    using MessagePack;
    using Newtonsoft.Json;
    using Score247Management.Core.Base;

    [MessagePackObject(keyAsPropertyName: true)]
    public class Team : BaseModel
    {
        public string Country { get; set; }

        public string CountryCode { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Flag { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public bool IsHome { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public TeamStatistic Statistic { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Abbreviation { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string qualifier { get; set; }
    }
}
namespace Score247Management.Core.Teams.Models
{
    using MessagePack;
    using Newtonsoft.Json;
    using Score247Management.Core.Base;

    [MessagePackObject(keyAsPropertyName: true)]
    public class Team : BaseModel
    {
        [SerializationConstructor, JsonConstructor]
        public Team(
            string id,
            string name,
            string country,
            string countryCode,
            string flag,
            bool isHome,
            TeamStatistic statistic,
            string abbreviation) : base(id, name)
        {
            Country = country;
            CountryCode = countryCode;
            Flag = flag;
            IsHome = isHome;
            Statistic = statistic;
            Abbreviation = abbreviation;
        }

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
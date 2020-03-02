namespace Score247Management.Core.Teams.Models
{
    using MessagePack;
    using Newtonsoft.Json;
    using Score247Management.Core.Base;

    [MessagePackObject(keyAsPropertyName: true)]
    public class Team : BaseModel
    {
        [JsonConstructor]
        public Team(string id, string name, string country, string countryCode, string abbreviation)
        {
            Id = id;
            Name = name;
            Country = country;
            CountryCode = countryCode;
            Abbreviation = abbreviation;
        }

        public Team(string value)
        {
            var match = JsonConvert.DeserializeObject<Team>(value);
            Id = match.Id;
            Name = match.Name;
            Country = match.Country;
            CountryCode = match.CountryCode;
            Abbreviation = match.Abbreviation;
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
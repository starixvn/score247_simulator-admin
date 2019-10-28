namespace Score247Management.Core.Leagues.Models
{
    using Newtonsoft.Json;

    public class LeagueCategory
    {
        public string Id { get; set; }
        public string Name { get; set; }

        [JsonProperty("country_code")]
        public string CountryCode { get; set; }

        public string Country2Code { get; set; }

        public string FlagSource { get; set; }
        public byte[] FlagData { get; set; }
    }
}
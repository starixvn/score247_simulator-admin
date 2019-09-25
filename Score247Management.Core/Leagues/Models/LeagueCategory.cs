namespace Score247Management.Core.Leagues.Models
{
    using Newtonsoft.Json;
    using Score247Management.Core.Base;

    public class LeagueCategory : BaseModel
    {
        [JsonProperty("country_code")]
        public string CountryCode { get; set; }

        public string Country2Code { get; set; }

        public string FlagSource { get; set; }
        public byte[] FlagData { get; set; }
    }
}
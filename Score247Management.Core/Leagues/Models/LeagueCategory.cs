namespace Score247Management.Core.Leagues.Models
{
    using Score247Management.Core.Base;

    public class LeagueCategory : BaseModel
    {
        public string CountryCode { get; set; }
        public string Country3Code { get; set; }

        public string FlagSource { get; set; }
        public byte[] FlagData { get; set; }
    }
}
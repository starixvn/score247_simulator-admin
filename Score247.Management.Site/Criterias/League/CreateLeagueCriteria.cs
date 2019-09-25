using Fanex.Data.Repository;

namespace Score247.Management.Site.Criterias.League
{
    public class CreateLeagueCriteria : NonQueryCommand
    {
        public string LeagueId { get; set; }
        public string LeagueName { get; set; }
        public int LeagueOrder { get; set; }
        public string CategoryId { get; set; }
        public string Country { get; set; }
        public string CountryCode { get; set; }
        public bool IsActive { get; set; }
        public string Language { get; set; }
        public string Region { get; set; }

        public override string GetSettingKey()
            => "Admin_League_InsertLeague";

        public override bool IsValid()
            => true;
    }
}
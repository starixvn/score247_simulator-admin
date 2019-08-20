using Fanex.Data.Repository;

namespace Score247Management.Criterias.Team
{
    public class CreateTeamCriteria : NonQueryCommand
    {
        public string Id { get; set; }
        public string TeamName { get; set; }
        public string Abbreviation { get; set; }
        public string Country { get; set; }
        public string CountryCode { get; set; }

        public override string GetSettingKey()
            => "Team_InsertTeam";

        public override bool IsValid()
            => true;
    }
}

using Fanex.Data.Repository;

namespace Score247Management.Criterias.League
{
    public class CreateLeagueCriteria : NonQueryCommand
    {
        public string LeagueId { get; set; }
        public string LeagueName { get; set; }
        public int LeagueOrder { get; set; }
        public string CategoryId { get; set; }

        public override string GetSettingKey()
            => "League_InsertLeague";

        public override bool IsValid()
            => true;
    }
}

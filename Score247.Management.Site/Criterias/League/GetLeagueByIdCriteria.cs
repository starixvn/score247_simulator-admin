using Fanex.Data.Repository;

namespace Score247.Management.Site.Criterias.League
{
    public class GetLeagueByIdCriteria : CriteriaBase
    {
        public string LeagueId { get; set; }

        public override string GetSettingKey()
            => "League_GetById";

        public override bool IsValid() => true;
    }
}
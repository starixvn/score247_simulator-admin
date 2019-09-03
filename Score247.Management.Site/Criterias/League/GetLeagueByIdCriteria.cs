using Fanex.Data.Repository;

namespace Score247.Management.Site.Criterias.League
{
    public class GetLeagueByIdCriteria : CriteriaBase
    {
        public string Id { get; set; }

        public override string GetSettingKey()
            => "League_GetLeagueById";

        public override bool IsValid() => true;
    }
}
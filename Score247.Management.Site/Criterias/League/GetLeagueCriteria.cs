using Fanex.Data.Repository;

namespace Score247.Management.Site.Criterias.League
{
    public class GetLeagueCriteria : CriteriaBase
    {
        public override string GetSettingKey()
            => "League_GetLeague";

        public override bool IsValid() => true;
    }
}
using Fanex.Data.Repository;

namespace Score247.Management.Site.Criterias.League
{
    public class GetLeagueCategoryCriteria : CriteriaBase
    {
        public override string GetSettingKey()
            => "League_GetCategory";

        public override bool IsValid()
            => true;
    }
}
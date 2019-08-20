using Fanex.Data.Repository;

namespace Score247Management.Criterias.League
{
    public class GetLeagueCriteria : CriteriaBase
    {
        public override string GetSettingKey()
            => "League_GetLeague";

        public override bool IsValid() => true;
    }
}

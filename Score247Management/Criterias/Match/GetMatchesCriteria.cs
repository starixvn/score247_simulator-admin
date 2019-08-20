using Fanex.Data.Repository;

namespace Score247Management.Criterias.Match
{
    public class GetMatchesCriteria : CriteriaBase
    {
        public override string GetSettingKey()
            => "Match_GetMatches";

        public override bool IsValid()
         => true;
    }
}

using Fanex.Data.Repository;

namespace Score247.Management.Site.Criterias.Images
{
    public class GetSportTeamCriteria : CriteriaBase
    {
        public override string GetSettingKey()
             => "SportTeam_GetNotFetched";

        public override bool IsValid()
            => true;
    }
}
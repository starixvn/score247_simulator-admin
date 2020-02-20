using Fanex.Data.Repository;

namespace Score247.Management.Site.Criterias.Images
{
    public class GetUnProcessRabitTeamsCriteria : CriteriaBase
    {
        public override string GetSettingKey()
            => "RapidTeams_GetUnprocess";

        public override bool IsValid()
            => true;
    }
}
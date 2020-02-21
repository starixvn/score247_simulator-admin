using Fanex.Data.Repository;
using Score247Management.Core._Shared.Enumerations;

namespace Score247.Management.Site.Criterias.Team
{
    public class GetTeamsCriteria : CriteriaBase
    {
        public GetTeamsCriteria()
        {
            LanguageCode = Language.en_US.DisplayName;
        }

        public string LanguageCode { get; }

        public override string GetSettingKey()
            => "Admin_Team_GetTeams";

        public override bool IsValid()
            => true;
    }
}
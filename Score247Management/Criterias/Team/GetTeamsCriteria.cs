using System;
using Fanex.Data.Repository;

namespace Score247Management.Criterias.Team
{
    public class GetTeamsCriteria : CriteriaBase
    {
        public override string GetSettingKey()
            => "Team_GetTeam";

        public override bool IsValid()
            => true;
    }
}

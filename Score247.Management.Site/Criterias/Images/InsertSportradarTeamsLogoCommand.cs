using Fanex.Data.Repository;
using Newtonsoft.Json;
using Score247Management.Core.Images;
using System.Collections.Generic;

namespace Score247.Management.Site.Criterias.Images
{
    public class InsertSportradarTeamsLogoCommand : NonQueryCommand
    {
        public InsertSportradarTeamsLogoCommand(IEnumerable<SportRadarTeamLogo> teams)
        {
            Teams = JsonConvert.SerializeObject(teams);
        }

        public string Teams { get; }

        public override string GetSettingKey()
            => "SportradarTeamsLogo_AddTeams";

        public override bool IsValid()
            => true;
    }
}
using Fanex.Data.Repository;
using Newtonsoft.Json;
using Score247Management.Core.Images;
using System.Collections.Generic;

namespace Score247.Management.Site.Criterias.Images
{
    public class InsertRapidTeamsCommand : NonQueryCommand
    {
        public InsertRapidTeamsCommand(IEnumerable<RapidTeam> teams)
        {
            Teams = JsonConvert.SerializeObject(teams);
        }

        public string Teams { get; }

        public override string GetSettingKey()
            => "RapidTeam_AddTeams";

        public override bool IsValid()
            => true;
    }
}
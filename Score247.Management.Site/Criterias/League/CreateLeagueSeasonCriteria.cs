using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fanex.Data.Repository;

namespace Score247.Management.Site.Criterias.League
{
    public class CreateLeagueSeasonCriteria : NonQueryCommand
    {
        public string SeasonId { get; set; }
        public string SeasonName { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string SeasonYear { get; set; }
        public string TournamentId { get; set; }

        public override string GetSettingKey()
            => "League_CreateLeagueSeason";

        public override bool IsValid()
            => true;
    }
}
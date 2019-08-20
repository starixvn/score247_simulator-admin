using System.Collections.Generic;
using Score247.Management.Site.Repositories.Match.Models;

namespace Score247.Management.Site.ViewModels.Match
{
    public class CreateMatchViewModel
    {
        public NewMatch NewMatch { get; set; }
        public IEnumerable<Score247Management.Core.Leagues.Models.League> Tournaments { get; set; }
        public IEnumerable<Score247Management.Core.Teams.Models.Team> Teams { get; set; }
        public Score247Management.Core.Teams.Models.Team HomeTeam { get; set; }
        public Score247Management.Core.Teams.Models.Team AwayTeam { get; set; }
    }
}

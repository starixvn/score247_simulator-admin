using System.Collections.Generic;

namespace Score247.Management.Site.ViewModels.League
{
    public class CreateLeagueViewModel
    {
        public Score247Management.Core.Leagues.Models.League League { get; set; }
        public IEnumerable<Score247Management.Core.Leagues.Models.LeagueCategory> LeagueCategories { get; set; }
    }
}
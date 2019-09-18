namespace Score247.Management.Site.ViewModels.Match
{
    using System.Collections.Generic;
    using Score247.Management.Site.Criterias.Match;
    using Score247.Management.Site.Models;

    public class MatchViewModel
    {
        public IEnumerable<Score247Management.Core.Matches.Models.Match> Matches { get; set; }
        public DateSearchModel DateSearchModel { get; set; }

        public CloneMatchModel CloneMatchModel { get; set; }
        public DeleteMatchByDateCriteria DeleteMatchByDateCriteria { get; set; }
        public string TimeOffsetString { get; set; }

        public List<CreateDeleteMatchesModel> CreateDeleteMatchesModels { get; set; }
        public string CreateMatchLeagueLimit { get; set; }

        public List<QuickDateSelectOption> QuickDateSelectOptions { get; set; }
    }
}
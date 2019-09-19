namespace Score247.Management.Site.ViewModels.Matches
{
    using Score247Management.Core.Matches.Models;
    using Score247Management.Core.Odds.Models;

    public class OddsEventViewModel
    {
        public Match Match { get; set; }

        public MatchOdds MatchOdds { get; set; }
        public MatchOddsMovement MatchOddsMovement { get; set; }
    }
}
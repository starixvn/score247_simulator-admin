namespace Score247Management.Core.Odds.Messages
{
    using System.Collections.Generic;
    using Score247Management.Core.Matches.Models;
    using Score247Management.Core.Odds.Models;

    public interface IOddsChangeMessage
    {
        IEnumerable<MatchOdds> MatchOddsList { get; }

        MatchEvent MatchEvent { get; }
    }

    public class OddsChangeMessage : IOddsChangeMessage
    {
        public OddsChangeMessage(
            IEnumerable<MatchOdds> matchOddsList,
            MatchEvent matchEvent = null)
        {
            MatchOddsList = matchOddsList;
            MatchEvent = matchEvent;
        }

        public IEnumerable<MatchOdds> MatchOddsList { get; }

        public MatchEvent MatchEvent { get; }
    }
}
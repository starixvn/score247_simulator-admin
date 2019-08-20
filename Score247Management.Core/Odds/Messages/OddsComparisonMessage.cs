namespace Score247Management.Core.Odds.Messages
{
    using System.Collections.Generic;
    using Score247Management.Core.Odds.Models;

    public interface IOddsComparisonMessage
    {
        string MatchId { get; }

        IEnumerable<BetTypeOdds> BetTypeOddsList { get; }
    }

    public class OddsComparisonMessage
    {
        public OddsComparisonMessage(
            string matchId,
            IEnumerable<BetTypeOdds> betTypeOddsList)
        {
            MatchId = matchId;
            BetTypeOddsList = betTypeOddsList;
        }

        public string MatchId { get; }

        public IEnumerable<BetTypeOdds> BetTypeOddsList { get; }
    }
}
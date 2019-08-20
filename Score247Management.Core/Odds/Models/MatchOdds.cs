namespace Score247Management.Core.Odds.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class MatchOdds
    {
        public MatchOdds(
            string matchId,
            IEnumerable<BetTypeOdds> betTypeOddsList,
            DateTime? lastUpdated = null)
        {
            MatchId = matchId;
            LastUpdated = lastUpdated;
            BetTypeOddsList = betTypeOddsList;
        }

        public string MatchId { get; }

        public DateTime? LastUpdated { get; private set; }

        public IEnumerable<BetTypeOdds> BetTypeOddsList { get; }

        public void SetLastUpdated(DateTime lastUpdated)
        {
            LastUpdated = lastUpdated;
        }

        public bool IsBetTypeOddsListEmpty()
            => BetTypeOddsList == null || !BetTypeOddsList.Any();
    }
}
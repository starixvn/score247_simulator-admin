namespace Score247Management.Core.Odds.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using MessagePack;

    [MessagePackObject(keyAsPropertyName: true)]
    public class MatchOdds
    {
        public string MatchId { get; set; }

        public DateTime? LastUpdated { get; set; }

        public IEnumerable<BetTypeOdds> BetTypeOddsList { get; set; }

        public void SetLastUpdated(DateTime lastUpdated)
        {
            LastUpdated = lastUpdated;
        }

        public bool IsBetTypeOddsListEmpty()
            => BetTypeOddsList == null || !BetTypeOddsList.Any();
    }
}
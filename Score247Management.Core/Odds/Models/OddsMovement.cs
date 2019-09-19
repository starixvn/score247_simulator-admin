namespace Score247Management.Core.Odds.Models
{
    using System;
    using System.Collections.Generic;
    using MessagePack;

    [MessagePackObject(keyAsPropertyName: true)]
    public class OddsMovement
    {
        public IEnumerable<BetOptionOdds> BetOptions { get; set; }

        public string MatchTime { get; set; }

        public DateTimeOffset UpdateTime { get; set; }

        public bool IsMatchStarted { get; set; }

        public int HomeScore { get; set; }

        public int AwayScore { get; set; }

        public void ResetLiveOddsToOpeningOdds()
        {
            if (BetOptions != null)
            {
                foreach (var betOption in BetOptions)
                {
                    betOption.ResetLiveOddsToOpeningOdds();
                }
            }
        }
    }
}
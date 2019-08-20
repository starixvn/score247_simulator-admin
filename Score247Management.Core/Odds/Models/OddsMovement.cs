namespace Score247Management.Core.Odds.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class OddsMovement
    {
        public OddsMovement(
            IEnumerable<BetOptionOdds> betOptions,
            string matchTime,
            DateTimeOffset updateTime,
            bool isMatchStated = false,
            int homeScore = 0,
            int awayScore = 0)
        {
            BetOptions = betOptions;
            MatchTime = matchTime;
            UpdateTime = updateTime;
            IsMatchStarted = isMatchStated;
            HomeScore = homeScore;
            AwayScore = awayScore;
        }

        public IEnumerable<BetOptionOdds> BetOptions { get; private set; }

        public string MatchTime { get; private set; }

        public int HomeScore { get; private set; }

        public int AwayScore { get; private set; }

        public bool IsMatchStarted { get; private set; }

        public DateTimeOffset UpdateTime { get; private set; }

        public void CalculateOddsTrend(IEnumerable<BetOptionOdds> prevBetOptions)
        {
            if (prevBetOptions.Count() != BetOptions.Count())
            {
                return;
            }

            var totalBetOptions = prevBetOptions.Count();

            for (int i = 0; i < totalBetOptions; i++)
            {
                BetOptions.ElementAt(i).CalculateOddsTrend(prevBetOptions.ElementAt(i).LiveOdds);
            }
        }

        public void ResetLiveOddsToOpeningOdds()
        {
            foreach (var betOption in BetOptions)
            {
                betOption.ResetLiveOddsToOpeningOdds();
            }
        }
    }
}
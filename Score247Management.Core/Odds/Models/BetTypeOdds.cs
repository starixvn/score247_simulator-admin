namespace Score247Management.Core.Odds.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using MessagePack;

    [MessagePackObject(keyAsPropertyName: true)]
    public class BetTypeOdds
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public Bookmaker Bookmaker { get; set; }

        public DateTime LastUpdatedTime { get; set; }

        public IEnumerable<BetOptionOdds> BetOptions { get; set; }

        public void SetLastUpdatedTime(DateTime lastUpdatedTime)
        {
            LastUpdatedTime = lastUpdatedTime;
        }

        public void AssignOpeningData(IEnumerable<BetOptionOdds> openingBetOptions)
        {
            if (openingBetOptions.Count() != BetOptions.Count())
            {
                return;
            }

            var totalBetOptions = openingBetOptions.Count();

            for (int i = 0; i < totalBetOptions; i++)
            {
                BetOptions.ElementAt(i).AssginOpeningData(openingBetOptions.ElementAt(i));
            }
        }

        public override bool Equals(object obj)
        {
            var betTypeOdds = obj as BetTypeOdds;

#pragma warning disable S1067 // Expressions should not be too complex
            if (betTypeOdds == null
                || betTypeOdds.Bookmaker == null
                || Bookmaker == null
                || !betTypeOdds.Bookmaker.Equals(Bookmaker)
                || BetOptions == null
                || betTypeOdds.BetOptions == null
                || BetOptions.Count() != betTypeOdds.BetOptions.Count())
#pragma warning restore S1067 // Expressions should not be too complex
            {
                return false;
            }

            var totalOptions = BetOptions.Count();
            for (int i = 0; i < totalOptions; i++)
            {
                if (!BetOptions.ElementAt(i).Equals(betTypeOdds.BetOptions.ElementAt(i)))
                {
                    return false;
                }
            }

            return true;
        }

        public override int GetHashCode()
            => Id.GetHashCode()
                + (Bookmaker == null ? 0 : Bookmaker.GetHashCode())
                + (BetOptions == null ? 0 : BetOptions.GetHashCode());
    }
}
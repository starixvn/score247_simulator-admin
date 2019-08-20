namespace Score247Management.Core.Odds.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class BetTypeOdds
    {
        public BetTypeOdds(
            int id,
            string name,
            Bookmaker bookmaker,
            DateTime lastUpdatedTime,
            IEnumerable<BetOptionOdds> betOptions)
        {
            Id = id;
            Name = name;
            Bookmaker = bookmaker;
            LastUpdatedTime = lastUpdatedTime;
            BetOptions = betOptions;
        }

        public int Id { get; }

        public string Name { get; }

        public Bookmaker Bookmaker { get; }

        public DateTime LastUpdatedTime { get; private set; }

        public IEnumerable<BetOptionOdds> BetOptions { get; private set; }

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
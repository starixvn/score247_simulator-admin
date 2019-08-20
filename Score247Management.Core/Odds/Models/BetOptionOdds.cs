namespace Score247Management.Core.Odds.Models
{
    public class BetOptionOdds
    {
        public BetOptionOdds(
            string type,
            decimal liveOdds,
            decimal openingOdds,
            string optionValue,
            string openingOptionValue)
        {
            Type = type;
            LiveOdds = liveOdds;
            OpeningOdds = openingOdds;
            OptionValue = optionValue;
            OpeningOptionValue = openingOptionValue;
        }

        public string Type { get; private set; }

        public decimal LiveOdds { get; private set; }

        public decimal OpeningOdds { get; private set; }

        public string OptionValue { get; private set; }

        public string OpeningOptionValue { get; private set; }

        public OddsTrend OddsTrend { get; private set; }

        public void AssginOpeningData(BetOptionOdds openingBetOption)
        {
            if (openingBetOption == null)
            {
                return;
            }

            OpeningOptionValue = string.IsNullOrWhiteSpace(openingBetOption.OpeningOptionValue)
                ? openingBetOption.OptionValue
                : openingBetOption.OpeningOptionValue;
            OpeningOdds = openingBetOption.OpeningOdds == 0
                    ? openingBetOption.LiveOdds
                    : openingBetOption.OpeningOdds;
            CalculateOddsTrend(OpeningOdds);
        }

        public void ResetLiveOddsToOpeningOdds()
        {
            LiveOdds = OpeningOdds;
            OddsTrend = OddsTrend.Neutral;
        }

        public override bool Equals(object obj)
        {
            var betOptionOdds = obj as BetOptionOdds;
            if (betOptionOdds == null)
            {
                return false;
            }

            return Type == betOptionOdds.Type
                && OptionValue == betOptionOdds.OptionValue
                && LiveOdds == betOptionOdds.LiveOdds
                && OpeningOdds == betOptionOdds.OpeningOdds;
        }

        public override int GetHashCode()
            => (Type ?? string.Empty).GetHashCode()
                + (OptionValue ?? string.Empty).GetHashCode()
                + LiveOdds.GetHashCode()
                + OpeningOdds.GetHashCode();

        public void CalculateOddsTrend(decimal prevOdds)
        {
            OddsTrend = prevOdds == LiveOdds
                ? OddsTrend.Neutral
#pragma warning disable S3358 // Ternary operators should not be nested
                : prevOdds > LiveOdds
                    ? OddsTrend.Down
                    : OddsTrend.Up;
#pragma warning restore S3358 // Ternary operators should not be nested
        }
    }
}
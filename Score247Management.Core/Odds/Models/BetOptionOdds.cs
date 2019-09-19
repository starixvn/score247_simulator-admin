namespace Score247Management.Core.Odds.Models
{
    using MessagePack;

    [MessagePackObject(keyAsPropertyName: true)]
    public class BetOptionOdds
    {
        public string Type { get; set; }

        public decimal LiveOdds { get; set; }

        public decimal OpeningOdds { get; set; }

        public string OptionValue { get; set; }

        public string OpeningOptionValue { get; set; }

        public OddsTrend OddsTrend { get; set; }

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
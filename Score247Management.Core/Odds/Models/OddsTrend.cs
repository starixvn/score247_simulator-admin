namespace Score247Management.Core.Odds.Models
{
    using MessagePack;
    using Score247Management.Core.Enumerations;

    [MessagePackObject(keyAsPropertyName: true)]
    public class OddsTrend : Enumeration
    {
        public static readonly OddsTrend Neutral = new OddsTrend(1, "neutral");
        public static readonly OddsTrend Up = new OddsTrend(2, "up");
        public static readonly OddsTrend Down = new OddsTrend(3, "down");

        public OddsTrend()
        {
        }

        public OddsTrend(byte value, string displayName)
            : base(value, displayName)
        {
        }
    }
}
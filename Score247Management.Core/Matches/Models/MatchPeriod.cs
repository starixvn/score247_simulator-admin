namespace Score247Management.Core.Matches.Models
{
    using MessagePack;
    using Score247Management.Core._Shared.Enumerations;

    [MessagePackObject]
    public class MatchPeriod
    {
        [Key(0)]
        public int HomeScore { get; set; }

        [Key(1)]
        public int AwayScore { get; set; }

        [Key(2)]
        public PeriodType PeriodType { get; set; }

        [Key(3)]
        public int? Number { get; set; }
    }
}
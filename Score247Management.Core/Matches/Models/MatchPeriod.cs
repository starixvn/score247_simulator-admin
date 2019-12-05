namespace Score247Management.Core.Matches.Models
{
    using MessagePack;
    using Score247Management.Core._Shared.Enumerations;

    [MessagePackObject(keyAsPropertyName: true)]
    public class MatchPeriod
    {
        public int HomeScore { get; set; }

        public int AwayScore { get; set; }

        public PeriodType PeriodType { get; set; }

        public int? Number { get; set; }
    }
}
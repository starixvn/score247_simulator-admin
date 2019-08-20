namespace Score247Management.Core.Matches.Models
{
    using Score247Management.Core._Shared.Enumerations;

    public class MatchPeriod
    {
        public int Id { get; set; }
        public int HomeScore { get; set; }

        public int AwayScore { get; set; }

        public PeriodType PeriodType { get; set; }

        public int Number { get; set; }
    }
}
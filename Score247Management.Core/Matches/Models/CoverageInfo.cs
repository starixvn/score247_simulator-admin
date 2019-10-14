namespace Score247Management.Core.Matches.Models
{
    public class CoverageInfo
    {
        public CoverageInfo()
        {
            Level = "bronze";
            LiveCoverage = true;
            Coverage = new Coverage();
        }

        public string Level { get; set; }
        public bool LiveCoverage { get; set; }
        public Coverage Coverage { get; set; }
    }

    public class Coverage
    {
        public Coverage()
        {
            BasicScore = true;
            KeyEvents = true;
            DetailedEvents = true;
            Lineups = true;
            PassesAndDuels = true;
            Commentary = true;
        }

        public bool BasicScore { get; set; }
        public bool KeyEvents { get; set; }
        public bool DetailedEvents { get; set; }
        public bool Lineups { get; set; }
        public bool PassesAndDuels { get; set; }
        public bool Commentary { get; set; }
    }
}
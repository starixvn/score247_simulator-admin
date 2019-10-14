namespace Score247Management.Core.Statistics.Models
{
    using Score247Management.Core.Teams.Models;
    using MessagePack;

    [MessagePackObject]
    public class Statistic
    {
        [Key(0)]
        public string MatchId { get; set; }

        [Key(1)]
        public TeamStatistic HomeStatistic { get; set; }

        [Key(2)]
        public TeamStatistic AwayStatistic { get; set; }
    }
}
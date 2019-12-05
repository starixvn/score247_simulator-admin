namespace Score247Management.Core.Statistics.Models
{
    using Score247Management.Core.Teams.Models;
    using MessagePack;

    [MessagePackObject(keyAsPropertyName: true)]
    public class Statistic
    {
        public string MatchId { get; set; }

        public TeamStatistic HomeStatistic { get; set; }

        public TeamStatistic AwayStatistic { get; set; }
    }
}
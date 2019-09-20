namespace Score247.Management.Site.ViewModels.Odds
{
    using System;

    public class OddsChange1X2RequestModel
    {
        public string MatchId { get; set; }
        public string BookmakerId { get; set; }
        public string HomeOdds { get; set; }
        public string AwayOdds { get; set; }
        public string DrawOdds { get; set; }
        public DateTime LastUpdate { get; set; }
    }
}
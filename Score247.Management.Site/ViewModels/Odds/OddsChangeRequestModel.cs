namespace Score247.Management.Site.ViewModels.Odds
{
    using System;

    public class OddsChangeRequestModel
    {
        public string MatchId { get; set; }
        public string BookmakerId { get; set; }
        public string HomeOdds { get; set; }
        public string AwayOdds { get; set; }
        public string DrawOdds { get; set; }
        public string OpeningHomeOdds { get; set; }
        public string OpeningAwayOdds { get; set; }
        public string OpeningDrawOdds { get; set; }
        public string HomeHDPOdds { get; set; }
        public string AwayHDPOdds { get; set; }
        public string HDPValue { get; set; }
        public string OpeningHomeHDPOdds { get; set; }
        public string OpeningAwayHDPOdds { get; set; }
        public string OpeningHDPValue { get; set; }
        public string OverOdds { get; set; }
        public string UnderOdds { get; set; }
        public string TotalOUOdds { get; set; }
        public string OpeningOverOdds { get; set; }
        public string OpeningUnderOdds { get; set; }
        public string OpeningTotalOUOdds { get; set; }

        public DateTime LastUpdate { get; set; }
        public int BetType { get; set; }
    }
}
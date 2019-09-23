namespace Score247.Management.Site.ViewModels.Odds
{
    public class OddsMovementRequestModel
    {
        public string MatchId { get; set; }
        public int BetType { get; set; }
        public string BetName { get; set; }
        public string BookmakerId { get; set; }
    }
}
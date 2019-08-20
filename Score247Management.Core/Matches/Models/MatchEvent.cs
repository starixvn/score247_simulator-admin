namespace Score247Management.Core.Matches.Models
{
    public class MatchEvent
    {
        public MatchEvent(string matchId, MatchResult matchResult, TimelineEvent timeline)
        {
            MatchId = matchId;
            MatchResult = matchResult;
            Timeline = timeline;
        }

        public string MatchId { get; }

        public MatchResult MatchResult { get; }

        public TimelineEvent Timeline { get; }
    }
}
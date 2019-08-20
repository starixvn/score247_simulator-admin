namespace Score247.Management.Site.ViewModels.Match
{
    using System.Collections.Generic;
    using Score247Management.Core.Matches.Models;
    using Score247.Management.Site.Models.Events;

    public class EventMatchViewModel
    {
        public Score247Management.Core.Matches.Models.Match Match { get; set; }
        public MatchPushEventDto MatchTimeEventDto { get; set; }
        public MatchPushEventDto MatchCardEventDto { get; set; }
        public MatchPushEventDto MatchPenaltyEventDto { get; set; }
        public MatchPushEventDto MatchGoalEventDto { get; set; }
        public MatchPushEventDto MatchInjuryExtraEventDto { get; set; }
        public IEnumerable<TimelineEvent> TimelineEvents { get; set; }
    }
}

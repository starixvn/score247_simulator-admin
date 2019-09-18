namespace Score247.Management.Site.ViewModels.Matches
{
    using System.Collections.Generic;
    using Score247.Management.Site.Models.Events;
    using Score247Management.Core.Matches.Models;
    using Score247Management.Core.Scenarios.Models;

    public class EventMatchViewModel
    {
        public Match Match { get; set; }
        public MatchPushEventDto MatchTimeEventDto { get; set; }
        public MatchPushEventDto MatchCardEventDto { get; set; }
        public MatchPushEventDto MatchPenaltyEventDto { get; set; }
        public MatchPushEventDto MatchGoalEventDto { get; set; }
        public MatchPushEventDto MatchInjuryExtraEventDto { get; set; }
        public IEnumerable<TimelineEvent> TimelineEvents { get; set; }

        public IEnumerable<Scenario> Scenarios { get; set; }
    }
}
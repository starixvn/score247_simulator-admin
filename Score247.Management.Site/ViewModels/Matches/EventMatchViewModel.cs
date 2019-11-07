using System;
using Score247Management.Core.Statistics.Models;

namespace Score247.Management.Site.ViewModels.Matches
{
    using System.Collections.Generic;
    using Score247.Management.Site.Helpers;
    using Score247.Management.Site.Models;
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
        public MatchPushEventDto MatchSubstitutionEventDto { get; set; }
        public MatchPushEventDto MatchMinorEventDto { get; set; }
        public IEnumerable<TimelineEvent> TimelineEvents { get; set; }

        public IEnumerable<Scenario> Scenarios { get; set; }
        public Statistic Statistic { get; set; }
        public MatchPitchViewLineups MatchPitchViewLineups { get; set; }
        public LineUpsDto LineUpsDto { get; set; }

        public EventMatchViewModel()
        {
        }

        public EventMatchViewModel(Match match, IEnumerable<TimelineEvent> timelineEvents, TimeSpan offset)
        {
            Match = match;
            TimelineEvents = timelineEvents;
            MatchCardEventDto = new MatchPushEventDto(match, offset);

            MatchCardEventDto.payload.timeline.player = new TimelinePlayer
            {
                id = UniqueGenerator.GetIdWithPattern("sr:player:"),
                name = NameGenerator.GeneratePlayerName()
            };
            MatchGoalEventDto = new MatchPushEventDto(match, offset);
            MatchGoalEventDto.payload.timeline.goal_scorer = new Goal_Scorer
            {
                id = UniqueGenerator.GetIdWithPattern("sr:player:"),
                name = NameGenerator.GeneratePlayerName()
            };
            MatchInjuryExtraEventDto = new MatchPushEventDto(match, offset);
            MatchPenaltyEventDto = new MatchPushEventDto(match, offset);
            MatchPenaltyEventDto.payload.timeline.goal_scorer = new Goal_Scorer
            {
                id = UniqueGenerator.GetIdWithPattern("sr:player:"),
                name = NameGenerator.GeneratePlayerName()
            };
            MatchTimeEventDto = new MatchPushEventDto(match, offset);

            MatchMinorEventDto = new MatchPushEventDto(match, offset);

            MatchSubstitutionEventDto = new MatchPushEventDto(match, offset);
            MatchSubstitutionEventDto.payload.timeline.player_in = new Player_In
            {
                id = UniqueGenerator.GetIdWithPattern("sr:player:"),
                name = NameGenerator.GeneratePlayerName()
            };
            MatchSubstitutionEventDto.payload.timeline.player_out = new Player_Out
            {
                id = UniqueGenerator.GetIdWithPattern("sr:player:"),
                name = NameGenerator.GeneratePlayerName()
            };
        }
    }
}
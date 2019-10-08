using System;
using System.Linq;
using Score247.Management.Site.Helpers;
using Score247.Management.Site.Models.Events;
using Score247Management.Core._Shared.Enumerations;
using Score247Management.Core.Matches.Models;

namespace Score247.Management.Site.Services.Matches.Creators
{
    public static class MatchTimeEventCreator
    {
        public static MatchPushEventDto CreateMatchStart(Match match, DateTime eventTime)
        {
            var matchEvent = new MatchPushEventDto(match);
            matchEvent.payload.timeline = new TimelineDto()
            {
                id = UniqueGenerator.GetIntId(),
                time = eventTime.ToUniversalTime(),
                home_score = 0,
                away_score = 0
            };

            matchEvent.payload.sport_event_status.status = "live";
            matchEvent.payload.sport_event_status.match_status = MatchStatus.FirstHaft.DisplayName;
            matchEvent.payload.sport_event_status.period = 1;
            matchEvent.payload.timeline.type = EventType.MatchStarted.DisplayName;

            return matchEvent;
        }

        public static MatchPushEventDto CreateFirstPeriodStart(Match match, DateTime eventTime)
        {
            var matchEvent = new MatchPushEventDto(match);
            matchEvent.payload.timeline = new TimelineDto()
            {
                id = UniqueGenerator.GetIntId(),
                time = eventTime.ToUniversalTime(),
                home_score = match.LatestTimeline != null ? match.LatestTimeline.HomeScore : 0,
                away_score = match.LatestTimeline != null ? match.LatestTimeline.AwayScore : 0
            };

            matchEvent.payload.sport_event_status.status = "live";
            matchEvent.payload.sport_event_status.match_status = MatchStatus.FirstHaft.DisplayName;
            matchEvent.payload.sport_event_status.period = 1;
            matchEvent.payload.timeline.type = EventType.PeriodStart.DisplayName;
            matchEvent.payload.timeline.period = 1;
            matchEvent.payload.timeline.period_name = MatchStatus.FirstHaft.DisplayName;
            matchEvent.payload.timeline.period_type = PeriodType.RegularPeriod.DisplayName;
            matchEvent.payload.sport_event_status.period_scores = matchEvent.payload.sport_event_status.period_scores.Append(new Period_Scores
            {
                home_score = 0,
                away_score = 0,
                type = "regular_period",
                number = 1
            });

            return matchEvent;
        }

        public static MatchPushEventDto CreateHalfTimeBreakStart(Match match, DateTime eventTime)
        {
            var matchEvent = new MatchPushEventDto(match);
            matchEvent.payload.timeline = new TimelineDto()
            {
                id = UniqueGenerator.GetIntId(),
                time = eventTime.ToUniversalTime(),
                home_score = match.LatestTimeline != null ? match.LatestTimeline.HomeScore : 0,
                away_score = match.LatestTimeline != null ? match.LatestTimeline.AwayScore : 0
            };

            matchEvent.payload.sport_event_status.status = "live";
            matchEvent.payload.sport_event_status.match_status = MatchStatus.Halftime.DisplayName;
            matchEvent.payload.sport_event_status.period = 1;
            matchEvent.payload.timeline.type = EventType.BreakStart.DisplayName;
            matchEvent.payload.timeline.period_name = MatchStatus.Pause.DisplayName;

            return matchEvent;
        }

        public static MatchPushEventDto CreateSecondPeriodStart(Match match, DateTime eventTime)
        {
            var matchEvent = new MatchPushEventDto(match);
            matchEvent.payload.timeline = new TimelineDto()
            {
                id = UniqueGenerator.GetIntId(),
                time = eventTime.ToUniversalTime(),
                home_score = match.LatestTimeline != null ? match.LatestTimeline.HomeScore : 0,
                away_score = match.LatestTimeline != null ? match.LatestTimeline.AwayScore : 0
            };

            matchEvent.payload.sport_event_status.status = "live";
            matchEvent.payload.sport_event_status.match_status = MatchStatus.SecondHaft.DisplayName;
            matchEvent.payload.sport_event_status.period = 2;
            matchEvent.payload.timeline.type = EventType.PeriodStart.DisplayName;
            matchEvent.payload.timeline.period = 2;
            matchEvent.payload.timeline.period_name = MatchStatus.SecondHaft.DisplayName;
            matchEvent.payload.timeline.period_type = PeriodType.RegularPeriod.DisplayName;
            matchEvent.payload.sport_event_status.period_scores = matchEvent.payload.sport_event_status.period_scores.Append(new Period_Scores
            {
                home_score = 0,
                away_score = 0,
                type = "regular_period",
                number = 2
            });

            return matchEvent;
        }

        public static MatchPushEventDto CreateAwaitingExtra(Match match, DateTime eventTime)
        {
            var matchEvent = new MatchPushEventDto(match);
            matchEvent.payload.timeline = new TimelineDto()
            {
                id = UniqueGenerator.GetIntId(),
                time = eventTime.ToUniversalTime(),
                home_score = match.LatestTimeline != null ? match.LatestTimeline.HomeScore : 0,
                away_score = match.LatestTimeline != null ? match.LatestTimeline.AwayScore : 0
            };

            matchEvent.payload.sport_event_status.status = "live";
            matchEvent.payload.sport_event_status.match_status = MatchStatus.AwaitingExtraTime.DisplayName;
            matchEvent.payload.sport_event_status.period = 1;
            matchEvent.payload.timeline.type = EventType.BreakStart.DisplayName;
            matchEvent.payload.timeline.period_name = "Awaiting extra";

            return matchEvent;
        }

        public static MatchPushEventDto CreateFirstExtraStart(Match match, DateTime eventTime)
        {
            var matchEvent = new MatchPushEventDto(match);
            matchEvent.payload.timeline = new TimelineDto()
            {
                id = UniqueGenerator.GetIntId(),
                time = eventTime.ToUniversalTime(),
                home_score = match.LatestTimeline != null ? match.LatestTimeline.HomeScore : 0,
                away_score = match.LatestTimeline != null ? match.LatestTimeline.AwayScore : 0
            };

            matchEvent.payload.sport_event_status.status = "live";
            matchEvent.payload.sport_event_status.match_status = MatchStatus.FirstHaftExtra.DisplayName;
            matchEvent.payload.sport_event_status.period = 1;
            matchEvent.payload.timeline.type = EventType.PeriodStart.DisplayName;
            matchEvent.payload.timeline.period = 1;
            matchEvent.payload.timeline.period_name = MatchStatus.FirstHaftExtra.DisplayName;
            matchEvent.payload.timeline.period_type = PeriodType.Overtime.DisplayName;
            matchEvent.payload.sport_event_status.period_scores = matchEvent.payload.sport_event_status.period_scores.Append(new Period_Scores
            {
                home_score = 0,
                away_score = 0,
                type = "overtime"
            });

            return matchEvent;
        }

        public static MatchPushEventDto CreateBreakExtraStart(Match match, DateTime eventTime)
        {
            var matchEvent = new MatchPushEventDto(match);
            matchEvent.payload.timeline = new TimelineDto()
            {
                id = UniqueGenerator.GetIntId(),
                time = eventTime.ToUniversalTime(),
                home_score = match.LatestTimeline != null ? match.LatestTimeline.HomeScore : 0,
                away_score = match.LatestTimeline != null ? match.LatestTimeline.AwayScore : 0
            };

            matchEvent.payload.sport_event_status.status = "live";
            matchEvent.payload.sport_event_status.match_status = MatchStatus.ExtraTimeHaftTime.DisplayName;
            matchEvent.payload.sport_event_status.period = 2;
            matchEvent.payload.timeline.type = EventType.BreakStart.DisplayName;
            matchEvent.payload.timeline.period_name = "Extra time halftime";

            return matchEvent;
        }

        public static MatchPushEventDto CreateSecondExtraStart(Match match, DateTime eventTime)
        {
            var matchEvent = new MatchPushEventDto(match);
            matchEvent.payload.timeline = new TimelineDto()
            {
                id = UniqueGenerator.GetIntId(),
                time = eventTime.ToUniversalTime(),
                home_score = match.LatestTimeline != null ? match.LatestTimeline.HomeScore : 0,
                away_score = match.LatestTimeline != null ? match.LatestTimeline.AwayScore : 0
            };

            matchEvent.payload.sport_event_status.status = "live";
            matchEvent.payload.sport_event_status.match_status = MatchStatus.SecondHaftExtra.DisplayName;
            matchEvent.payload.sport_event_status.period = 2;
            matchEvent.payload.timeline.type = EventType.PeriodStart.DisplayName;
            matchEvent.payload.timeline.period = 2;
            matchEvent.payload.timeline.period_name = MatchStatus.SecondHaftExtra.DisplayName;
            matchEvent.payload.timeline.period_type = PeriodType.Overtime.DisplayName;

            return matchEvent;
        }

        public static MatchPushEventDto CreateAwaitingPenalty(Match match, DateTime eventTime)
        {
            var matchEvent = new MatchPushEventDto(match);
            matchEvent.payload.timeline = new TimelineDto()
            {
                id = UniqueGenerator.GetIntId(),
                time = eventTime.ToUniversalTime(),
                home_score = match.LatestTimeline != null ? match.LatestTimeline.HomeScore : 0,
                away_score = match.LatestTimeline != null ? match.LatestTimeline.AwayScore : 0
            };

            matchEvent.payload.sport_event_status.status = "live";
            matchEvent.payload.sport_event_status.match_status = MatchStatus.AwaitingPenalties.DisplayName;
            matchEvent.payload.sport_event_status.period = 1;
            matchEvent.payload.timeline.type = EventType.BreakStart.DisplayName;
            matchEvent.payload.timeline.period_name = MatchStatus.AwaitingPenalties.DisplayName;

            return matchEvent;
        }

        public static MatchPushEventDto CreatePenaltyStart(Match match, DateTime eventTime)
        {
            var matchEvent = new MatchPushEventDto(match);
            matchEvent.payload.timeline = new TimelineDto()
            {
                id = UniqueGenerator.GetIntId(),
                time = eventTime.ToUniversalTime(),
                home_score = match.LatestTimeline != null ? match.LatestTimeline.HomeScore : 0,
                away_score = match.LatestTimeline != null ? match.LatestTimeline.AwayScore : 0
            };

            matchEvent.payload.sport_event_status.status = "live";
            matchEvent.payload.sport_event_status.match_status = MatchStatus.Penalties.DisplayName;
            matchEvent.payload.sport_event_status.period = 1;
            matchEvent.payload.timeline.type = EventType.PeriodStart.DisplayName;
            matchEvent.payload.timeline.period = 1;
            matchEvent.payload.timeline.period_name = MatchStatus.Penalties.DisplayName;
            matchEvent.payload.timeline.period_type = PeriodType.Penalties.DisplayName;
            matchEvent.payload.sport_event_status.period_scores = matchEvent.payload.sport_event_status.period_scores.Append(new Period_Scores
            {
                home_score = 0,
                away_score = 0,
                type = "penalties"
            });

            return matchEvent;
        }

        public static MatchPushEventDto CreateMatchEndNormal(Match match, DateTime eventTime)
        {
            var matchEvent = new MatchPushEventDto(match);
            matchEvent.payload.timeline = new TimelineDto()
            {
                id = UniqueGenerator.GetIntId(),
                time = eventTime.ToUniversalTime(),
                home_score = match.LatestTimeline != null ? match.LatestTimeline.HomeScore : 0,
                away_score = match.LatestTimeline != null ? match.LatestTimeline.AwayScore : 0
            };

            matchEvent.payload.sport_event_status.status = "closed";
            matchEvent.payload.sport_event_status.match_status = MatchStatus.Ended.DisplayName;
            matchEvent.payload.timeline.type = EventType.MatchEnded.DisplayName;
            matchEvent.payload.timeline.match_clock = "90:00";

            return matchEvent;
        }

        public static MatchPushEventDto CreateMatchEndExtra(Match match, DateTime eventTime)
        {
            var matchEvent = new MatchPushEventDto(match);
            matchEvent.payload.timeline = new TimelineDto()
            {
                id = UniqueGenerator.GetIntId(),
                time = eventTime.ToUniversalTime(),
                home_score = match.LatestTimeline != null ? match.LatestTimeline.HomeScore : 0,
                away_score = match.LatestTimeline != null ? match.LatestTimeline.AwayScore : 0
            };

            matchEvent.payload.sport_event_status.status = "closed";
            matchEvent.payload.sport_event_status.match_status = MatchStatus.EndedExtraTime.DisplayName;
            matchEvent.payload.timeline.type = EventType.MatchEnded.DisplayName;
            matchEvent.payload.timeline.match_clock = "120:00";

            return matchEvent;
        }

        public static MatchPushEventDto CreateMatchEndPenalty(Match match, DateTime eventTime)
        {
            var matchEvent = new MatchPushEventDto(match);
            matchEvent.payload.timeline = new TimelineDto()
            {
                id = UniqueGenerator.GetIntId(),
                time = eventTime.ToUniversalTime(),
                home_score = match.LatestTimeline != null ? match.LatestTimeline.HomeScore : 0,
                away_score = match.LatestTimeline != null ? match.LatestTimeline.AwayScore : 0
            };

            matchEvent.payload.sport_event_status.status = "closed";
            matchEvent.payload.sport_event_status.match_status = MatchStatus.EndedAfterPenalties.DisplayName;
            matchEvent.payload.sport_event_status.period = 1;
            matchEvent.payload.timeline.type = EventType.MatchEnded.DisplayName;
            matchEvent.payload.timeline.match_clock = "120:00";

            var penaltyResult = matchEvent.payload.sport_event_status.period_scores.FirstOrDefault(x => x.type == PeriodType.Penalties.DisplayName);
            if (penaltyResult.home_score > penaltyResult.away_score)
            {
                matchEvent.payload.sport_event_status.winner_id = match.Teams.FirstOrDefault(x => x.IsHome).Id;
            }
            else
            {
                matchEvent.payload.sport_event_status.winner_id = match.Teams.FirstOrDefault(x => !x.IsHome).Id;
            }

            return matchEvent;
        }
    }
}
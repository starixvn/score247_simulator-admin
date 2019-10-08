using System;
using Score247.Management.Site.Helpers;
using Score247.Management.Site.Models.Events;
using Score247Management.Core._Shared.Enumerations;
using Score247Management.Core.Matches.Models;

namespace Score247.Management.Site.Services.Matches
{
    public static class MatchInjuryExtraEventCreator
    {
        public static MatchPushEventDto CreateInjuryExtraEvent(Match match, DateTime eventTime, int injuryTime, string matchStatus)
        {
            var matchEvent = new MatchPushEventDto(match);

            matchEvent.payload.sport_event_status.match_status = matchStatus;

            matchEvent.payload.timeline = new TimelineDto()
            {
                id = UniqueGenerator.GetIntId(),
                time = eventTime.ToUniversalTime(),
                period = 1,
                period_name = PeriodType.RegularPeriod.DisplayName,
                stoppage_time = "1",
                type = EventType.InjuryTimeShown.DisplayName,
                match_time = 45,
                match_clock = "45:00",
                injury_time_announced = injuryTime,
                home_score = match.LatestTimeline != null ? match.LatestTimeline.HomeScore : 0,
                away_score = match.LatestTimeline != null ? match.LatestTimeline.AwayScore : 0
            };

            if (matchEvent.payload.sport_event_status.match_status == MatchStatus.SecondHaft.DisplayName)
            {
                matchEvent.payload.timeline.period = 2;
                matchEvent.payload.timeline.match_time = 90;
                matchEvent.payload.timeline.match_clock = "90:00";
            }

            return matchEvent;
        }

        public static MatchPushEventDto CreateFirstHalfInjuryExtraEvent(Match match, DateTime eventTime, int injuryTime)
        {
            var matchEvent = MatchInjuryExtraEventCreator.CreateInjuryExtraEvent(match, eventTime, injuryTime, MatchStatus.FirstHaft.DisplayName);

            return matchEvent;
        }

        public static MatchPushEventDto CreateSecondHalfInjuryExtraEvent(Match match, DateTime eventTime, int injuryTime)
        {
            var matchEvent = MatchInjuryExtraEventCreator.CreateInjuryExtraEvent(match, eventTime, injuryTime, MatchStatus.SecondHaft.DisplayName);

            return matchEvent;
        }
    }
}
using System.Linq;
using Score247.Management.Site.Models.Events;
using Score247Management.Core._Shared.Enumerations;
using Score247Management.Core.Enumerations;
using Score247Management.Core.Matches.Models;

namespace Score247.Management.Site.Models.DataMappers
{
    public static class MatchMapper
    {
        public static MatchResult MapMatchResult(PushEventPayload payload)
        {
            var result = new MatchResult
            {
                AggregateAwayScore = payload.sport_event_status.aggregate_away_score ?? 0,
                AggregateHomeScore = payload.sport_event_status.aggregate_home_score ?? 0,
                AggregateWinnerId = payload.sport_event_status.aggregate_winner_id,
                AwayScore = payload.sport_event_status.away_score ?? 0,
                HomeScore = payload.sport_event_status.home_score ?? 0,
                EventStatus = Enumeration.FromDisplayName<MatchStatus>(payload.sport_event_status.status),
                MatchStatus = Enumeration.FromDisplayName<MatchStatus>(payload.sport_event_status.match_status),
                MatchTime = payload.timeline.match_time ?? 0,
                Period = payload.sport_event_status.period ?? 0,
                WinnerId = payload.sport_event_status.winner_id,
                MatchPeriods = payload.sport_event_status.period_scores.Select(MapMatchPeriod)
            };

            return result;
        }

        public static MatchPeriod MapMatchPeriod(Period_Scores period_Scores)
        {
            var matchPeriod = new MatchPeriod
            {
                AwayScore = period_Scores.away_score,
                HomeScore = period_Scores.home_score,
                Number = period_Scores.number,
                PeriodType = Enumeration.FromDisplayName<PeriodType>(period_Scores.type)
            };

            return matchPeriod;
        }
    }
}
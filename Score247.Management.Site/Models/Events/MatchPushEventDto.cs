using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Score247.Management.Site.Helpers;
using Score247Management.Core.Matches.Models;

namespace Score247.Management.Site.Models.Events
{
    public class MatchPushEventDto
    {
        public PushEventPayload payload { get; set; }

        public PushEventMetaData metadata { get; set; }

        public MatchPushEventDto()
        {
        }

        public MatchPushEventDto(Match match)
        {
            payload = new PushEventPayload(match);
            metadata = new PushEventMetaData(match);
        }

        public MatchPushEventDto(Match match, TimeSpan offset)
        {
            payload = new PushEventPayload(match, offset);
            metadata = new PushEventMetaData(match);
        }
    }

    public class PushEventPayload
    {
        public Sport_Event_Status sport_event_status { get; set; }

        [JsonProperty(PropertyName = "event")]
        public TimelineDto timeline { get; set; }

        public PushEventPayload()
        {
        }

        public PushEventPayload(Match match)
        {
            var matchPeriodsScores = match.MatchResult.MatchPeriods == null ? new List<Period_Scores>() : match.MatchResult.MatchPeriods?.Select(x => new Period_Scores { home_score = x.HomeScore, away_score = x.AwayScore, number = x.Number, type = x.PeriodType.DisplayName });
            sport_event_status = new Sport_Event_Status()
            {
                aggregate_away_score = match.LatestTimeline != null ? match.LatestTimeline.AwayScore : 0,
                aggregate_home_score = match.LatestTimeline != null ? match.LatestTimeline.HomeScore : 0,
                aggregate_winner_id = match.MatchResult.AggregateWinnerId,
                away_score = match.LatestTimeline != null ? match.LatestTimeline.AwayScore : 0,
                clock = new Clock { match_time = "00:00" },
                home_score = match.LatestTimeline != null ? match.LatestTimeline.HomeScore : 0,
                match_status = match.MatchResult.MatchStatus.DisplayName,
                period = match.MatchResult.Period,
                period_scores = matchPeriodsScores,
                status = "live",
                winner_id = match.MatchResult.WinnerId
            };

            var latestTime = match.LatestTimeline != null ? match.LatestTimeline.Time : match.EventDate;
            timeline = new TimelineDto()
            {
                time = latestTime.ToUniversalTime(),
                hour = new TimeSpan(latestTime.ToUniversalTime().Hour, latestTime.ToUniversalTime().Minute + 1, 0),
                period_type = "regular_period",
                period = match.MatchResult.Period
            };
        }

        public PushEventPayload(Match match, TimeSpan offset)
        {
            var matchPeriodsScores = match.MatchResult.MatchPeriods == null ? new List<Period_Scores>() : match.MatchResult.MatchPeriods?.Select(x => new Period_Scores { home_score = x.HomeScore, away_score = x.AwayScore, number = x.Number, type = x.PeriodType.DisplayName });
            sport_event_status = new Sport_Event_Status()
            {
                aggregate_away_score = match.LatestTimeline != null ? match.LatestTimeline.AwayScore : 0,
                aggregate_home_score = match.LatestTimeline != null ? match.LatestTimeline.HomeScore : 0,
                aggregate_winner_id = match.MatchResult?.AggregateWinnerId,
                away_score = match.LatestTimeline != null ? match.LatestTimeline.AwayScore : 0,
                clock = new Clock { match_time = "00:00" },
                home_score = match.LatestTimeline != null ? match.LatestTimeline.HomeScore : 0,
                match_status = match.MatchResult?.MatchStatus?.DisplayName,
                period = match.MatchResult != null ? match.MatchResult.Period : 0,
                period_scores = matchPeriodsScores,
                status = "live",
                winner_id = match.MatchResult?.WinnerId
            };

            var latestTime = match.LatestTimeline != null ? match.LatestTimeline.Time.Add(offset) : match.EventDate;
            timeline = new TimelineDto()
            {
                time = latestTime.ToUniversalTime(),
                hour = new TimeSpan(latestTime.ToUniversalTime().Hour, latestTime.ToUniversalTime().Minute + 1, 0)
            };
        }
    }

    public class PushEventMetaData
    {
        public string tournament_id { get; set; }

        public string season_id { get; set; }

        public string sport_event_id { get; set; }

        public string sport_id { get; set; }

        public PushEventMetaData()
        {
        }

        public PushEventMetaData(Match match)
        {
            season_id = "";
            sport_event_id = match.Id;
            sport_id = "1";
            tournament_id = match.League.Id;
        }
    }
}
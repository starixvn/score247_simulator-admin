namespace Score247.Management.Site.Services.Matchs
{
    using System;
    using System.Linq;
    using Score247.Management.Site.Helpers;
    using Score247.Management.Site.Models.Events;
    using Score247Management.Core._Shared.Enumerations;
    using Score247Management.Core.Matches.Models;

    public static class MatchPenaltyEventCreator
    {
        public static MatchPushEventDto CreatePenaltyEvent(Match match, DateTime eventTime, string team, string playerName, string status)
        {
            var matchEvent = new MatchPushEventDto(match);
            matchEvent.payload.timeline = new TimelineDto()
            {
                id = UniqueGenerator.GetIntId(),
                time = eventTime.ToUniversalTime(),
                home_score = match.LatestTimeline != null ? match.LatestTimeline.HomeScore : 0,
                away_score = match.LatestTimeline != null ? match.LatestTimeline.AwayScore : 0,
                team = team,
                type = EventType.PenaltyShootout.DisplayName,
                period_type = PeriodType.Penalties.DisplayName,
                status = status
            };

            matchEvent.payload.timeline.player = new TimelinePlayer()
            {
                id = UniqueGenerator.GetIdWithPattern("sr:player:"),
                name = playerName
            };

            var periodScores = matchEvent.payload.sport_event_status.period_scores.ToList();
            if (matchEvent.payload.timeline.status == "scored")
            {
                var periodScoresPosition = periodScores.FindIndex(x => x.type == PeriodType.Penalties.DisplayName);
                if (matchEvent.payload.timeline.team == "home")
                {
                    periodScores[periodScoresPosition].home_score++;
                }
                else
                {
                    periodScores[periodScoresPosition].away_score++;
                }
            }

            matchEvent.payload.sport_event_status.period_scores = periodScores;

            return matchEvent;
        }
    }
}
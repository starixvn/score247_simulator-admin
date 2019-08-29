using System;
using Score247.Management.Site.Helpers;
using Score247.Management.Site.Models.Events;
using Score247Management.Core.Matches.Models;

namespace Score247.Management.Site.Services.Matchs
{
    public static class MatchCardEventCreator
    {
        public static MatchPushEventDto CreateCardEvent(Match match, DateTime eventTime, int matchTime, string team, string playerName, string eventType, int extraTime)
        {
            var matchEvent = new MatchPushEventDto(match);
            matchEvent.payload.timeline = new TimelineDto()
            {
                id = UniqueGenerator.GetIntId(),
                time = eventTime.ToUniversalTime(),
                home_score = match.LatestTimeline != null ? match.LatestTimeline.HomeScore : 0,
                away_score = match.LatestTimeline != null ? match.LatestTimeline.AwayScore : 0,
                team = team,
                match_time = matchTime,
                match_clock = matchTime.ToString() + ":00",
                type = eventType,
                period = match.MatchResult.Period
            };

            if (extraTime > 0)
            {
                matchEvent.payload.timeline.stoppage_time = extraTime.ToString();
            }
            matchEvent.payload.timeline.player = new TimelinePlayer()
            {
                id = UniqueGenerator.GetIdWithPattern("sr:player:"),
                name = playerName
            };

            return matchEvent;
        }

        public static MatchPushEventDto CreateRedCardEvent(Match match, DateTime eventTime, int matchTime, string team, string playerName, int extraTime)
        {
            var matchEvent = MatchCardEventCreator.CreateCardEvent(match, eventTime, matchTime, team, playerName, "red_card", extraTime);

            return matchEvent;
        }

        public static MatchPushEventDto CreateYellowCardEvent(Match match, DateTime eventTime, int matchTime, string team, string playerName, int extraTime)
        {
            var matchEvent = MatchCardEventCreator.CreateCardEvent(match, eventTime, matchTime, team, playerName, "yellow_card", extraTime);

            return matchEvent;
        }

        public static MatchPushEventDto CreateYellowRedCardEvent(Match match, DateTime eventTime, int matchTime, string team, string playerName, int extraTime)
        {
            var matchEvent = MatchCardEventCreator.CreateCardEvent(match, eventTime, matchTime, team, playerName, "yellow_red_card", extraTime);

            return matchEvent;
        }

        //public static MatchPushEventDto CreateRedCardEventOvertime(Match match, DateTime eventTime, int matchTime, string team, string playerName)
        //{
        //    var matchEvent = MatchCardEventCreator.CreateCardEvent(match, eventTime, matchTime, team, playerName, "red_card", extraTime);

        //    return matchEvent;
        //}
        //public static MatchPushEventDto CreateYellowCardOvertime(Match match, DateTime eventTime, int matchTime, string team, string playerName)
        //{
        //    var matchEvent = MatchCardEventCreator.CreateCardEvent(match, eventTime, matchTime, team, playerName, "yellow_card", extraTime);

        //    return matchEvent;
        //}
        //public static MatchPushEventDto CreateYellowRedCardOvertime(Match match, DateTime eventTime, int matchTime, string team, string playerName)
        //{
        //    var matchEvent = MatchCardEventCreator.CreateCardEvent(match, eventTime, matchTime, team, playerName, "yellow_red_card", extraTime);

        //    return matchEvent;
        //}
    }
}
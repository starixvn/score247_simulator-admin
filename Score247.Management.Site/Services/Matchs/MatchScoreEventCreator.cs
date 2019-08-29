namespace Score247.Management.Site.Services.Matchs
{
    using System;
    using System.Linq;
    using Score247.Management.Site.Helpers;
    using Score247.Management.Site.Models.Events;
    using Score247Management.Core._Shared.Enumerations;
    using Score247Management.Core.Matches.Models;

    public static class MatchScoreEventCreator
    {
        public static MatchPushEventDto CreateNormalScore(Match match, DateTime eventTime, int matchTime, string team, string scoredPlayer, bool isOwnScore)
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
                type = EventType.ScoreChange.DisplayName
            };
            matchEvent.payload.timeline.goal_scorer = new Goal_Scorer()
            {
                id = UniqueGenerator.GetIdWithPattern("sr:player:"),
                name = scoredPlayer
            };
            if (isOwnScore)
            {
                matchEvent.payload.timeline.goal_scorer.method = "own_goal";
            }
            var periodScores = matchEvent.payload.sport_event_status.period_scores.ToList();
            var periodScoresPosition = periodScores.FindIndex(x => x.type == PeriodType.RegularPeriod.DisplayName && x.number == match.MatchResult.Period);
            if (periodScoresPosition < 0)
            {
                periodScores.Add(new Period_Scores
                {
                    away_score = team == "home" ? 0 : 1,
                    home_score = team == "home" ? 1 : 0,
                    type = PeriodType.RegularPeriod.DisplayName,
                    number = match.LatestTimeline.Period
                });
            }
            else
            {
                if ((team == "home" && !isOwnScore) || (team == "away" && isOwnScore))
                {
                    matchEvent.payload.timeline.home_score++;
                    periodScores[periodScoresPosition].home_score++;
                }
                else
                {
                    matchEvent.payload.timeline.away_score++;
                    periodScores[periodScoresPosition].away_score++;
                }
            }

            matchEvent.payload.sport_event_status.period_scores = periodScores;

            matchEvent.payload.sport_event_status.home_score = matchEvent.payload.timeline.home_score;
            matchEvent.payload.sport_event_status.away_score = matchEvent.payload.timeline.away_score;

            return matchEvent;
        }

        public static MatchPushEventDto CreateNormalScoreInExtra(Match match, DateTime eventTime, int matchTime, string team, string scoredPlayer, bool isOwnGoal, int extraTime)
        {
            var matchEvent = MatchScoreEventCreator.CreateNormalScore(match, eventTime, matchTime, team, scoredPlayer, isOwnGoal);
            matchEvent.payload.timeline.stoppage_time = extraTime.ToString();
            return matchEvent;
        }

        public static MatchPushEventDto CreateAssistScore(Match match, DateTime eventTime, int matchTime, string team, string scoredPlayer, string assistPlayer)
        {
            var matchEvent = MatchScoreEventCreator.CreateNormalScore(match, eventTime, matchTime, team, scoredPlayer, false);

            matchEvent.payload.timeline.assist = new Assist()
            {
                id = UniqueGenerator.GetIdWithPattern("sr:player:"),
                name = assistPlayer,
                type = "primary"
            };

            return matchEvent;
        }

        public static MatchPushEventDto CreateAssistScoreInExtra(Match match, DateTime eventTime, int matchTime, string team, string scoredPlayer, string assistPlayer, int extraTime)
        {
            var matchEvent = MatchScoreEventCreator.CreateAssistScore(match, eventTime, matchTime, team, scoredPlayer, assistPlayer);

            matchEvent.payload.timeline.assist = new Assist()
            {
                id = UniqueGenerator.GetIdWithPattern("sr:player:"),
                name = assistPlayer,
                type = "primary"
            };

            matchEvent.payload.timeline.stoppage_time = extraTime.ToString();

            return matchEvent;
        }

        public static MatchPushEventDto CreatePenaltyScore(Match match, DateTime eventTime, int matchTime, string team, string scoredPlayer)
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
                type = EventType.ScoreChange.DisplayName
            };
            matchEvent.payload.timeline.goal_scorer = new Goal_Scorer()
            {
                id = UniqueGenerator.GetIdWithPattern("sr:player:"),
                name = scoredPlayer
            };
            matchEvent.payload.timeline.goal_scorer.method = "penalty";

            var periodScores = matchEvent.payload.sport_event_status.period_scores.ToList();
            var periodScoresPosition = periodScores.FindIndex(x => x.type == PeriodType.RegularPeriod.DisplayName && x.number == match.MatchResult.Period);
            if (periodScoresPosition < 0)
            {
                periodScores.Add(new Period_Scores
                {
                    away_score = team == "home" ? 0 : 1,
                    home_score = team == "home" ? 1 : 0,
                    type = PeriodType.RegularPeriod.DisplayName,
                    number = match.LatestTimeline.Period
                });
            }
            else
            {
                if (team == "home")
                {
                    matchEvent.payload.timeline.home_score++;
                    periodScores[periodScoresPosition].home_score++;
                }
                else
                {
                    matchEvent.payload.timeline.away_score++;
                    periodScores[periodScoresPosition].away_score++;
                }
            }

            matchEvent.payload.sport_event_status.period_scores = periodScores;

            matchEvent.payload.sport_event_status.home_score = matchEvent.payload.timeline.home_score;
            matchEvent.payload.sport_event_status.away_score = matchEvent.payload.timeline.away_score;

            return matchEvent;
        }

        public static MatchPushEventDto CreatePenaltyScoreInExtra(Match match, DateTime eventTime, int matchTime, string team, string scoredPlayer, int extraTime)
        {
            var matchEvent = MatchScoreEventCreator.CreatePenaltyScore(match, eventTime, matchTime, team, scoredPlayer);
            matchEvent.payload.timeline.stoppage_time = extraTime.ToString();

            return matchEvent;
        }

        public static MatchPushEventDto CreateOvertimeScore(Match match, DateTime eventTime, int matchTime, string team, string scoredPlayer, bool isOwnScore)
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
                type = EventType.ScoreChange.DisplayName,
                period_type = PeriodType.Overtime.DisplayName
            };
            matchEvent.payload.timeline.goal_scorer = new Goal_Scorer()
            {
                id = UniqueGenerator.GetIdWithPattern("sr:player:"),
                name = scoredPlayer
            };
            if (isOwnScore)
            {
                matchEvent.payload.timeline.goal_scorer.method = "own_goal";
            }
            var periodScores = matchEvent.payload.sport_event_status.period_scores.ToList();
            var periodScoresPosition = periodScores.FindIndex(x => x.type == PeriodType.Overtime.DisplayName);
            if (periodScoresPosition < 0)
            {
                periodScores.Add(new Period_Scores
                {
                    away_score = team == "home" ? 0 : 1,
                    home_score = team == "home" ? 1 : 0,
                    type = PeriodType.Overtime.DisplayName
                });
            }
            else
            {
                if (team == "home")
                {
                    matchEvent.payload.timeline.home_score++;
                    periodScores[periodScoresPosition].home_score++;
                }
                else
                {
                    matchEvent.payload.timeline.away_score++;
                    periodScores[periodScoresPosition].away_score++;
                }
            }

            matchEvent.payload.sport_event_status.period_scores = periodScores;

            matchEvent.payload.sport_event_status.home_score = matchEvent.payload.timeline.home_score;
            matchEvent.payload.sport_event_status.away_score = matchEvent.payload.timeline.away_score;

            return matchEvent;
        }

        public static MatchPushEventDto CreateOvertimeAssistScore(Match match, DateTime eventTime, int matchTime, string team, string scoredPlayer, bool isOwnScore, string assistPlayer)
        {
            var matchEvent = MatchScoreEventCreator.CreateOvertimeScore(match, eventTime, matchTime, team, scoredPlayer, isOwnScore);

            matchEvent.payload.timeline.assist = new Assist()
            {
                id = UniqueGenerator.GetIdWithPattern("sr:player:"),
                name = assistPlayer,
                type = "primary"
            };
            return matchEvent;
        }

        public static MatchPushEventDto CreateOvertimePenaltyScore(Match match, DateTime eventTime, int matchTime, string team, string scoredPlayer)
        {
            var matchEvent = MatchScoreEventCreator.CreateOvertimeScore(match, eventTime, matchTime, team, scoredPlayer, false);
            matchEvent.payload.timeline.goal_scorer.method = "penalty";

            return matchEvent;
        }
    }
}
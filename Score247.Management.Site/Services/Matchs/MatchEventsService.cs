using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Score247.Management.Site.Helpers;
using Score247.Management.Site.Models.Events;
using Score247.Management.Site.Repositories.Matches;
using Score247Management.Core._Shared.Enumerations;

namespace Score247.Management.Site.Services.Matchs
{
    public interface IMatchEventsService
    {
        Task CreateTimeEventPartial(string id, IFormCollection collection, TimeSpan offset);

        Task CreateScoreEventPartial(string id, IFormCollection collection, TimeSpan offset);

        Task CreatePenaltyShootEventPartial(string id, IFormCollection collection, TimeSpan offset);

        Task CreatCardFaultEventPartial(string id, IFormCollection collection, TimeSpan offset);

        Task CreateInjuryExtraTimeEventPartial(string id, IFormCollection collection, TimeSpan offset);
    }

    public class MatchEventsService : IMatchEventsService
    {
        private readonly ISimulatorService simulatorService;
        private readonly IMatchService matchService;

        public MatchEventsService(IMatchService matchService, ISimulatorService simulatorService)
        {
            this.matchService = matchService;
            this.simulatorService = simulatorService;
        }

        public async Task CreatCardFaultEventPartial(string id, IFormCollection collection, TimeSpan offset)
        {
            var matchRepository = new MatchRepository();
            var match = await matchRepository.GetMatchById(id);
            var requestBody = new MatchPushEventDto(match);
            var eventDate = Convert.ToDateTime(collection["payload.timeline.time"]);
            var eventTimeSpan = TimeSpan.Parse(collection["payload.timeline.hour"]);
            var eventTime = new DateTime(eventDate.Year, eventDate.Month, eventDate.Day, eventTimeSpan.Hours, eventTimeSpan.Minutes, eventTimeSpan.Seconds, DateTimeKind.Utc).Subtract(offset).ToUniversalTime();
            requestBody.payload.timeline = new TimelineDto()
            {
                id = UniqueGenerator.GetIntId(),
                time = eventTime,
                period = match.MatchResult.Period,
                type = collection["payload.timeline.type"],
                team = collection["payload.timeline.team"],
                match_time = Int32.Parse(collection["payload.timeline.match_time"]),
                match_clock = collection["payload.timeline.match_time"] + ":00",
                home_score = match.LatestTimeline != null ? match.LatestTimeline.HomeScore : 0,
                away_score = match.LatestTimeline != null ? match.LatestTimeline.AwayScore : 0,
                period_type = "regular_period"
            };

            if (collection["payload.timeline.period_type"] == "overtime")
            {
                requestBody.payload.timeline.period_type = "overtime";
                requestBody.payload.timeline.stoppage_time = collection["payload.timeline.stoppage_time"];
            }

            if (!string.IsNullOrWhiteSpace(collection["payload.timeline.player.name"]))
            {
                requestBody.payload.timeline.player = new TimelinePlayer()
                {
                    id = UniqueGenerator.GetIdWithPattern("sr:player:"),
                    name = collection["payload.timeline.player.name"]
                };
            }

            await simulatorService.SendMatchEvent(requestBody);

            await matchService.UpdateLiveMatch(match.Id);
        }

        public async Task CreateInjuryExtraTimeEventPartial(string id, IFormCollection collection, TimeSpan offset)
        {
            var matchRepository = new MatchRepository();
            var match = await matchRepository.GetMatchById(id);
            var requestBody = new MatchPushEventDto(match);

            requestBody.payload.sport_event_status.match_status = collection["payload.sport_event_status.match_status"];

            var eventDate = Convert.ToDateTime(collection["payload.timeline.time"]);
            var eventTimeSpan = TimeSpan.Parse(collection["payload.timeline.hour"]);
            var eventTime = new DateTime(eventDate.Year, eventDate.Month, eventDate.Day, eventTimeSpan.Hours, eventTimeSpan.Minutes, eventTimeSpan.Seconds, DateTimeKind.Utc).Subtract(offset).ToUniversalTime();
            requestBody.payload.timeline = new TimelineDto()
            {
                id = UniqueGenerator.GetIntId(),
                time = eventTime,
                period = 1,
                type = EventType.InjuryTimeShown.DisplayName,
                match_time = 45,
                match_clock = "45:00",
                injury_time_announced = Int32.Parse(collection["payload.timeline.injury_time_announced"]),
                home_score = match.LatestTimeline != null ? match.LatestTimeline.HomeScore : 0,
                away_score = match.LatestTimeline != null ? match.LatestTimeline.AwayScore : 0,
                period_type = "regular_period"
            };

            if (requestBody.payload.sport_event_status.match_status == MatchStatus.SecondHaft.DisplayName)
            {
                requestBody.payload.timeline.period = 2;
                requestBody.payload.timeline.match_time = 90;
                requestBody.payload.timeline.match_clock = "90:00";
            }

            if (requestBody.payload.sport_event_status.match_status == MatchStatus.FirstHaftExtra.DisplayName)
            {
                requestBody.payload.timeline.period = 1;
                requestBody.payload.timeline.match_time = 105;
                requestBody.payload.timeline.match_clock = "105:00";
                requestBody.payload.timeline.period_type = "overtime";
            }

            if (requestBody.payload.sport_event_status.match_status == MatchStatus.SecondHaftExtra.DisplayName)
            {
                requestBody.payload.timeline.period = 2;
                requestBody.payload.timeline.match_time = 120;
                requestBody.payload.timeline.match_clock = "120:00";
                requestBody.payload.timeline.period_type = "overtime";
            }

            await simulatorService.SendMatchEvent(requestBody);

            await matchService.UpdateLiveMatch(match.Id);
        }

        public async Task CreatePenaltyShootEventPartial(string id, IFormCollection collection, TimeSpan offset)
        {
            var matchRepository = new MatchRepository();
            var match = await matchRepository.GetMatchById(id);
            var requestBody = new MatchPushEventDto(match);
            var eventDate = Convert.ToDateTime(collection["payload.timeline.time"]);
            var eventTimeSpan = TimeSpan.Parse(collection["payload.timeline.hour"]);
            var eventTime = new DateTime(eventDate.Year, eventDate.Month, eventDate.Day, eventTimeSpan.Hours, eventTimeSpan.Minutes, eventTimeSpan.Seconds, DateTimeKind.Utc).Subtract(offset).ToUniversalTime();
            requestBody.payload.timeline = new TimelineDto()
            {
                id = UniqueGenerator.GetIntId(),
                time = eventTime
            };

            requestBody.payload.timeline.home_score = match.LatestTimeline != null ? match.LatestTimeline.HomeScore : 0;
            requestBody.payload.timeline.away_score = match.LatestTimeline != null ? match.LatestTimeline.AwayScore : 0;

            if (!string.IsNullOrWhiteSpace(collection["payload.timeline.goal_scorer.name"]))
            {
                requestBody.payload.timeline.player = new TimelinePlayer()
                {
                    id = UniqueGenerator.GetIdWithPattern("sr:player:"),
                    name = collection["payload.timeline.goal_scorer.name"]
                };
            }
            requestBody.payload.timeline.type = EventType.PenaltyShootout.DisplayName;
            requestBody.payload.timeline.team = collection["payload.timeline.team"];
            requestBody.payload.timeline.period_type = PeriodType.Penalties.DisplayName;
            requestBody.payload.timeline.status = collection["payload.timeline.status"];

            var periodScores = requestBody.payload.sport_event_status.period_scores.ToList();
            if (requestBody.payload.timeline.status == "scored")
            {
                var periodScoresPosition = periodScores.FindIndex(x => x.type == PeriodType.Penalties.DisplayName);
                if (requestBody.payload.timeline.team == "home")
                {
                    periodScores[periodScoresPosition].home_score++;
                }
                else
                {
                    periodScores[periodScoresPosition].away_score++;
                }
            }

            requestBody.payload.sport_event_status.period_scores = periodScores;

            await simulatorService.SendMatchEvent(requestBody);
        }

        public async Task CreateScoreEventPartial(string id, IFormCollection collection, TimeSpan offset)
        {
            var matchRepository = new MatchRepository();
            var match = await matchRepository.GetMatchById(id);
            var requestBody = new MatchPushEventDto(match);

            var eventDate = Convert.ToDateTime(collection["payload.timeline.time"]);
            var eventTimeSpan = TimeSpan.Parse(collection["payload.timeline.hour"]);
            var eventTime = new DateTime(eventDate.Year, eventDate.Month, eventDate.Day, eventTimeSpan.Hours, eventTimeSpan.Minutes, eventTimeSpan.Seconds, DateTimeKind.Utc).Subtract(offset).ToUniversalTime();
            requestBody.payload.timeline = new TimelineDto()
            {
                id = UniqueGenerator.GetIntId(),
                time = eventTime,
                period_type = "regular_period",
                period = match.MatchResult.Period
            };
            if (!string.IsNullOrWhiteSpace(collection["payload.timeline.assist.name"]))
            {
                requestBody.payload.timeline.assist = new Assist()
                {
                    id = UniqueGenerator.GetIdWithPattern("sr:player:"),
                    name = collection["payload.timeline.assist.name"],
                    type = "primary"
                };
            }

            if (!string.IsNullOrWhiteSpace(collection["payload.timeline.goal_scorer.name"]))
            {
                requestBody.payload.timeline.goal_scorer = new Goal_Scorer()
                {
                    id = UniqueGenerator.GetIdWithPattern("sr:player:"),
                    name = collection["payload.timeline.goal_scorer.name"]
                };
            }
            requestBody.payload.timeline.type = EventType.ScoreChange.DisplayName;
            requestBody.payload.timeline.team = collection["payload.timeline.team"];
            requestBody.payload.timeline.match_time = Int32.Parse(collection["payload.timeline.match_time"]);
            requestBody.payload.timeline.match_clock = collection["payload.timeline.match_time"] + ":00";
            requestBody.payload.timeline.home_score = match.LatestTimeline != null ? match.LatestTimeline.HomeScore : 0;
            requestBody.payload.timeline.away_score = match.LatestTimeline != null ? match.LatestTimeline.AwayScore : 0;

            var eventType = collection["payload.timeline.type"];
            if (eventType != "normal_goal")
            {
                requestBody.payload.timeline.goal_scorer.method = eventType;
            }
            var periodType = collection["payload.timeline.period_type"];
            if (periodType == "extra_time")
            {
                requestBody.payload.timeline.period_type = "overtime";
                requestBody.payload.timeline.stoppage_time = collection["payload.timeline.stoppage_time"];
            }
            var isHomeScore = (requestBody.payload.timeline.team == "home" && eventType != "own_goal") ||
                (requestBody.payload.timeline.team == "away" && eventType == "own_goal");

            if (isHomeScore)
            {
                requestBody.payload.timeline.home_score++;
            }
            else
            {
                requestBody.payload.timeline.away_score++;
            }
            var periodScores = requestBody.payload.sport_event_status.period_scores.ToList();
            if (periodType == PeriodType.Overtime.DisplayName)
            {
                var periodScoresPosition = periodScores.FindIndex(x => x.type == PeriodType.Overtime.DisplayName);
                if (periodScoresPosition < 0)
                {
                    periodScores.Add(new Period_Scores
                    {
                        away_score = isHomeScore ? 0 : 1,
                        home_score = isHomeScore ? 1 : 0,
                        type = PeriodType.Overtime.DisplayName
                    });
                }
                else
                {
                    if (isHomeScore)
                    {
                        periodScores[periodScoresPosition].home_score++;
                    }
                    else
                    {
                        periodScores[periodScoresPosition].away_score++;
                    }
                }
            }
            else
            {
                var periodScoresPosition = periodScores.FindIndex(x => x.type == PeriodType.RegularPeriod.DisplayName && x.number == match.MatchResult.Period);
                if (periodScoresPosition < 0)
                {
                    periodScores.Add(new Period_Scores
                    {
                        away_score = isHomeScore ? 0 : 1,
                        home_score = isHomeScore ? 1 : 0,
                        type = PeriodType.RegularPeriod.DisplayName,
                        number = match.LatestTimeline.Period
                    });
                }
                else
                {
                    if (isHomeScore)
                    {
                        periodScores[periodScoresPosition].home_score++;
                    }
                    else
                    {
                        periodScores[periodScoresPosition].away_score++;
                    }
                }
            }

            requestBody.payload.sport_event_status.period_scores = periodScores;

            requestBody.payload.sport_event_status.home_score = requestBody.payload.timeline.home_score;
            requestBody.payload.sport_event_status.away_score = requestBody.payload.timeline.away_score;

            await simulatorService.SendMatchEvent(requestBody);

            await matchService.UpdateLiveMatch(match.Id);
        }

        public async Task CreateTimeEventPartial(string id, IFormCollection collection, TimeSpan offset)
        {
            var matchRepository = new MatchRepository();
            var match = await matchRepository.GetMatchById(id);
            var requestBody = new MatchPushEventDto(match);
            var eventDate = Convert.ToDateTime(collection["payload.timeline.time"]);
            var eventTimeSpan = TimeSpan.Parse(collection["payload.timeline.hour"]);
            var eventTime = new DateTime(eventDate.Year, eventDate.Month, eventDate.Day, eventTimeSpan.Hours, eventTimeSpan.Minutes, eventTimeSpan.Seconds, DateTimeKind.Utc).Subtract(offset).ToUniversalTime();
            requestBody.payload.timeline = new TimelineDto()
            {
                id = UniqueGenerator.GetIntId(),
                time = eventTime,
                period_type = "regular_period",
                period = match.MatchResult.Period
            };
            requestBody.payload.timeline.home_score = match.LatestTimeline != null ? match.LatestTimeline.HomeScore : 0;
            requestBody.payload.timeline.away_score = match.LatestTimeline != null ? match.LatestTimeline.AwayScore : 0;

            requestBody.payload.sport_event_status.aggregate_away_score =
                collection.ContainsKey("aggregate_away_score")
                ? int.Parse(collection["aggregate_away_score"])
                : 0;
            requestBody.payload.sport_event_status.aggregate_home_score =
                collection.ContainsKey("aggregate_home_score")
                ? int.Parse(collection["aggregate_home_score"])
                : 0;
            if (collection["aggregate_winner_id"] == "home")
            {
                requestBody.payload.sport_event_status.aggregate_winner_id = match.Teams.FirstOrDefault(x => x.IsHome).Id;
            }
            else if (collection["aggregate_winner_id"] == "away")
            {
                requestBody.payload.sport_event_status.aggregate_winner_id = match.Teams.FirstOrDefault(x => !x.IsHome).Id;
            }
            else
            {
                requestBody.payload.sport_event_status.aggregate_winner_id = null;
                requestBody.payload.sport_event_status.aggregate_away_score = 0;
                requestBody.payload.sport_event_status.aggregate_home_score = 0;
            }

            var eventType = collection["payload.timeline.type"];

            if (eventType == "match_started")
            {
                requestBody.payload.sport_event_status.status = "live";
                requestBody.payload.sport_event_status.match_status = MatchStatus.FirstHaft.DisplayName;
                requestBody.payload.sport_event_status.period = 1;
                requestBody.payload.timeline.type = EventType.MatchStarted.DisplayName;
                requestBody.payload.sport_event_status.period_scores = requestBody.payload.sport_event_status.period_scores.Append(new Period_Scores
                {
                    home_score = 0,
                    away_score = 0,
                    type = "regular_period",
                    number = 1
                });
            }
            else if (eventType == "1st_period_start")
            {
                requestBody.payload.sport_event_status.status = "live";
                requestBody.payload.sport_event_status.match_status = MatchStatus.FirstHaft.DisplayName;
                requestBody.payload.sport_event_status.period = 1;
                requestBody.payload.timeline.type = EventType.PeriodStart.DisplayName;
                requestBody.payload.timeline.period = 1;
                requestBody.payload.timeline.period_name = MatchStatus.FirstHaft.DisplayName;
                requestBody.payload.timeline.period_type = PeriodType.RegularPeriod.DisplayName;
                if (requestBody.payload.sport_event_status.period_scores.FirstOrDefault(x => x.number == 1 && x.type == PeriodType.RegularPeriod.DisplayName) == null)
                {
                    requestBody.payload.sport_event_status.period_scores = requestBody.payload.sport_event_status.period_scores.Append(new Period_Scores
                    {
                        home_score = 0,
                        away_score = 0,
                        type = "regular_period",
                        number = 1
                    });
                }
            }
            else if (eventType == "break_start")
            {
                requestBody.payload.sport_event_status.status = "live";
                requestBody.payload.sport_event_status.match_status = MatchStatus.Halftime.DisplayName;
                requestBody.payload.sport_event_status.period = 1;
                requestBody.payload.timeline.period_type = null;
                requestBody.payload.timeline.type = EventType.BreakStart.DisplayName;
                requestBody.payload.timeline.period_name = MatchStatus.Pause.DisplayName;
                if (requestBody.payload.sport_event_status.period_scores.FirstOrDefault(x => x.number == 1 && x.type == PeriodType.RegularPeriod.DisplayName) == null)
                {
                    requestBody.payload.sport_event_status.period_scores = requestBody.payload.sport_event_status.period_scores.Append(new Period_Scores
                    {
                        home_score = 0,
                        away_score = 0,
                        type = "regular_period",
                        number = 1
                    });
                }
            }
            else if (eventType == "2nd_period_start")
            {
                requestBody.payload.sport_event_status.status = "live";
                requestBody.payload.sport_event_status.match_status = MatchStatus.SecondHaft.DisplayName;
                requestBody.payload.sport_event_status.period = 2;
                requestBody.payload.timeline.type = EventType.PeriodStart.DisplayName;
                requestBody.payload.timeline.period = 2;
                requestBody.payload.timeline.period_name = MatchStatus.SecondHaft.DisplayName;
                requestBody.payload.timeline.period_type = PeriodType.RegularPeriod.DisplayName;
                requestBody.payload.sport_event_status.period_scores = requestBody.payload.sport_event_status.period_scores.Append(new Period_Scores
                {
                    home_score = 0,
                    away_score = 0,
                    type = "regular_period",
                    number = 2
                });
            }
            else if (eventType == "await_extra")
            {
                requestBody.payload.sport_event_status.status = "live";
                requestBody.payload.sport_event_status.match_status = MatchStatus.AwaitingExtraTime.DisplayName;
                requestBody.payload.sport_event_status.period = 1;
                requestBody.payload.timeline.period_type = null;
                requestBody.payload.timeline.type = EventType.BreakStart.DisplayName;
                requestBody.payload.timeline.period_name = "Awaiting extra";
                if (requestBody.payload.sport_event_status.period_scores.FirstOrDefault(x => x.type == PeriodType.Overtime.DisplayName) == null)
                {
                    requestBody.payload.sport_event_status.period_scores = requestBody.payload.sport_event_status.period_scores.Append(new Period_Scores
                    {
                        home_score = 0,
                        away_score = 0,
                        type = "overtime"
                    });
                }
            }
            else if (eventType == "1st_extra_start")
            {
                requestBody.payload.sport_event_status.status = "live";
                requestBody.payload.sport_event_status.match_status = MatchStatus.FirstHaftExtra.DisplayName;
                requestBody.payload.sport_event_status.period = 1;
                requestBody.payload.timeline.type = EventType.PeriodStart.DisplayName;
                requestBody.payload.timeline.period = 1;
                requestBody.payload.timeline.period_name = MatchStatus.FirstHaftExtra.DisplayName;
                requestBody.payload.timeline.period_type = PeriodType.Overtime.DisplayName;
                if (requestBody.payload.sport_event_status.period_scores.FirstOrDefault(x => x.type == PeriodType.Overtime.DisplayName) == null)
                {
                    requestBody.payload.sport_event_status.period_scores = requestBody.payload.sport_event_status.period_scores.Append(new Period_Scores
                    {
                        home_score = 0,
                        away_score = 0,
                        type = "overtime"
                    });
                }
            }
            else if (eventType == "break_extra")
            {
                requestBody.payload.sport_event_status.status = "live";
                requestBody.payload.sport_event_status.match_status = MatchStatus.ExtraTimeHaftTime.DisplayName;
                requestBody.payload.sport_event_status.period = 2;
                requestBody.payload.timeline.period_type = null;
                requestBody.payload.timeline.type = EventType.BreakStart.DisplayName;
                requestBody.payload.timeline.period_name = "Extra time halftime";
                if (requestBody.payload.sport_event_status.period_scores.FirstOrDefault(x => x.type == PeriodType.Overtime.DisplayName) == null)
                {
                    requestBody.payload.sport_event_status.period_scores = requestBody.payload.sport_event_status.period_scores.Append(new Period_Scores
                    {
                        home_score = 0,
                        away_score = 0,
                        type = "overtime"
                    });
                }
            }
            else if (eventType == "2nd_extra_start")
            {
                requestBody.payload.sport_event_status.status = "live";
                requestBody.payload.sport_event_status.match_status = MatchStatus.SecondHaftExtra.DisplayName;
                requestBody.payload.sport_event_status.period = 2;
                requestBody.payload.timeline.type = EventType.PeriodStart.DisplayName;
                requestBody.payload.timeline.period = 2;
                requestBody.payload.timeline.period_name = MatchStatus.SecondHaftExtra.DisplayName;
                requestBody.payload.timeline.period_type = PeriodType.Overtime.DisplayName;
                if (requestBody.payload.sport_event_status.period_scores.FirstOrDefault(x => x.type == PeriodType.Overtime.DisplayName) == null)
                {
                    requestBody.payload.sport_event_status.period_scores = requestBody.payload.sport_event_status.period_scores.Append(new Period_Scores
                    {
                        home_score = 0,
                        away_score = 0,
                        type = "overtime"
                    });
                }
            }
            else if (eventType == "await_penalty")
            {
                requestBody.payload.sport_event_status.status = "live";
                requestBody.payload.sport_event_status.match_status = MatchStatus.AwaitingPenalties.DisplayName;
                requestBody.payload.sport_event_status.period = 1;
                requestBody.payload.timeline.period_type = null;
                requestBody.payload.timeline.type = EventType.BreakStart.DisplayName;
                requestBody.payload.timeline.period_name = MatchStatus.AwaitingPenalties.DisplayName;
                if (requestBody.payload.sport_event_status.period_scores.FirstOrDefault(x => x.type == PeriodType.Penalties.DisplayName) == null)
                {
                    requestBody.payload.sport_event_status.period_scores = requestBody.payload.sport_event_status.period_scores.Append(new Period_Scores
                    {
                        home_score = 0,
                        away_score = 0,
                        type = "penalties"
                    });
                }
            }
            else if (eventType == "penalty_start")
            {
                requestBody.payload.sport_event_status.status = "live";
                requestBody.payload.sport_event_status.match_status = MatchStatus.Penalties.DisplayName;
                requestBody.payload.sport_event_status.period = 1;
                requestBody.payload.timeline.type = EventType.PeriodStart.DisplayName;
                requestBody.payload.timeline.period = 1;
                requestBody.payload.timeline.period_name = MatchStatus.Penalties.DisplayName;
                requestBody.payload.timeline.period_type = PeriodType.Penalties.DisplayName;
                if (requestBody.payload.sport_event_status.period_scores.FirstOrDefault(x => x.type == PeriodType.Penalties.DisplayName) == null)
                {
                    requestBody.payload.sport_event_status.period_scores = requestBody.payload.sport_event_status.period_scores.Append(new Period_Scores
                    {
                        home_score = 0,
                        away_score = 0,
                        type = "penalties"
                    });
                }
            }
            else if (eventType == "match_end_normal")
            {
                requestBody.payload.sport_event_status.status = "closed";
                requestBody.payload.sport_event_status.match_status = MatchStatus.Ended.DisplayName;
                requestBody.payload.timeline.type = EventType.MatchEnded.DisplayName;
                requestBody.payload.timeline.match_clock = "90:00";
                requestBody.payload.timeline.period_type = null;
                if (requestBody.payload.sport_event_status.period_scores.FirstOrDefault(x => x.number == 1 && x.type == PeriodType.RegularPeriod.DisplayName) == null)
                {
                    requestBody.payload.sport_event_status.period_scores = requestBody.payload.sport_event_status.period_scores.Append(new Period_Scores
                    {
                        home_score = 0,
                        away_score = 0,
                        type = "regular_period",
                        number = 1
                    });
                }
                if (requestBody.payload.sport_event_status.period_scores.FirstOrDefault(x => x.number == 2 && x.type == PeriodType.RegularPeriod.DisplayName) == null)
                {
                    requestBody.payload.sport_event_status.period_scores = requestBody.payload.sport_event_status.period_scores.Append(new Period_Scores
                    {
                        home_score = 0,
                        away_score = 0,
                        type = "regular_period",
                        number = 2
                    });
                }
            }
            else if (eventType == "match_end_extra")
            {
                requestBody.payload.sport_event_status.status = "closed";
                requestBody.payload.sport_event_status.match_status = MatchStatus.EndedExtraTime.DisplayName;
                requestBody.payload.timeline.type = EventType.MatchEnded.DisplayName;
                requestBody.payload.timeline.match_clock = "120:00";
                requestBody.payload.timeline.period_type = null;
                if (requestBody.payload.sport_event_status.period_scores.FirstOrDefault(x => x.type == PeriodType.Overtime.DisplayName) == null)
                {
                    requestBody.payload.sport_event_status.period_scores = requestBody.payload.sport_event_status.period_scores.Append(new Period_Scores
                    {
                        home_score = 0,
                        away_score = 0,
                        type = "overtime"
                    });
                }
            }
            else if (eventType == "match_end_pen")
            {
                requestBody.payload.sport_event_status.status = "closed";
                requestBody.payload.sport_event_status.match_status = MatchStatus.EndedAfterPenalties.DisplayName;
                requestBody.payload.sport_event_status.period = 1;
                requestBody.payload.timeline.period_type = null;
                requestBody.payload.timeline.type = EventType.MatchEnded.DisplayName;
                requestBody.payload.timeline.match_clock = "120:00";
                if (requestBody.payload.sport_event_status.period_scores.FirstOrDefault(x => x.type == PeriodType.Penalties.DisplayName) == null)
                {
                    requestBody.payload.sport_event_status.period_scores = requestBody.payload.sport_event_status.period_scores.Append(new Period_Scores
                    {
                        home_score = 0,
                        away_score = 0,
                        type = "penalties"
                    });
                }

                var penaltyResult = requestBody.payload.sport_event_status.period_scores.FirstOrDefault(x => x.type == PeriodType.Penalties.DisplayName);
                if (penaltyResult.home_score > penaltyResult.away_score)
                {
                    requestBody.payload.sport_event_status.winner_id = match.Teams.FirstOrDefault(x => x.IsHome).Id;
                }
                else
                {
                    requestBody.payload.sport_event_status.winner_id = match.Teams.FirstOrDefault(x => !x.IsHome).Id;
                }
            }
            else
            {
                return;
            }

            await simulatorService.SendMatchEvent(requestBody);

            await matchService.UpdateLiveMatch(match.Id);
        }
    }
}
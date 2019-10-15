using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Score247.Management.Site.Helpers;
using Score247.Management.Site.Models.Events;
using Score247.Management.Site.Repositories.Matches;
using Score247Management.Core._Shared.Enumerations;

namespace Score247.Management.Site.Services.Matches
{
    public interface IMatchEventsService
    {
        Task CreateTimeEventPartial(string id, IFormCollection collection, TimeSpan offset);

        Task CreateScoreEventPartial(string id, IFormCollection collection, TimeSpan offset);

        Task CreatePenaltyShootEventPartial(string id, IFormCollection collection, TimeSpan offset);

        Task CreateCardFaultEventPartial(string id, IFormCollection collection, TimeSpan offset);

        Task CreateInjuryExtraTimeEventPartial(string id, IFormCollection collection, TimeSpan offset);

        Task CreateSubstitutionEventPartial(string id, IFormCollection collection, TimeSpan offset);

        Task CreateMinorEventPartial(string id, IFormCollection collection, TimeSpan offset);
    }

    public class MatchEventsService : IMatchEventsService
    {
        private readonly ISimulatorService simulatorService;
        private readonly IMatchService matchService;
        private readonly ITimelineService timelineService;

        public MatchEventsService(IMatchService matchService, ISimulatorService simulatorService, ITimelineService timelineService)
        {
            this.matchService = matchService;
            this.simulatorService = simulatorService;
            this.timelineService = timelineService;
        }

        public async Task CreateCardFaultEventPartial(string id, IFormCollection collection, TimeSpan offset)
        {
            var requestBody = await InitEvent(id, collection, offset);
            if (collection["payload.timeline.period_type"] == "overtime")
            {
                requestBody.payload.timeline.period_type = "overtime";
                requestBody.payload.timeline.stoppage_time = collection["payload.timeline.stoppage_time"];
            }

            requestBody.payload.timeline.player = new TimelinePlayer()
            {
                id = UniqueGenerator.GetIdWithPattern("sr:player:"),
                name = collection["payload.timeline.player.name"]
            };

            await simulatorService.SendMatchEvent(requestBody);
            await timelineService.SendMatchTimeline(id);
            await matchService.UpdateLiveMatch(id);
        }

        public async Task CreateInjuryExtraTimeEventPartial(string id, IFormCollection collection, TimeSpan offset)
        {
            var requestBody = await InitEvent(id, collection, offset);

            requestBody.payload.timeline.type = EventType.InjuryTimeShown.DisplayName;
            requestBody.payload.timeline.match_time = 45;
            requestBody.payload.timeline.match_clock = "45:00";
            requestBody.payload.timeline.injury_time_announced = int.Parse(collection["payload.timeline.injury_time_announced"]);

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

            await timelineService.SendMatchTimeline(id);
            await matchService.UpdateLiveMatch(id);
        }

        public async Task<MatchPushEventDto> InitEvent(string id, IFormCollection collection, TimeSpan offset)
        {
            var matchRepository = new MatchRepository();
            var match = await matchRepository.GetMatchById(id);
            var requestBody = new MatchPushEventDto(match);
            var eventDate = Convert.ToDateTime(collection["payload.timeline.time"]);
            var eventTimeSpan = TimeSpan.Parse(collection["payload.timeline.hour"]);
            var eventTime = new DateTime(eventDate.Year, eventDate.Month, eventDate.Day, eventTimeSpan.Hours, eventTimeSpan.Minutes, eventTimeSpan.Seconds, DateTimeKind.Utc).Subtract(offset).ToUniversalTime();
            var isSuccessParseMatchTime = int.TryParse(collection["payload.timeline.match_time"], out var matchTime);
            var isNotDuplicateEvent = string.IsNullOrWhiteSpace(collection["payload.timeline.id"]);
            requestBody.payload.timeline = new TimelineDto
            {
                id =
                    isNotDuplicateEvent
                        ? UniqueGenerator.GetIntId()
                        : long.Parse(collection["payload.timeline.id"]),
                time = eventTime,
                type = collection["payload.timeline.type"],
                match_time = isSuccessParseMatchTime ? matchTime : 0,
                match_clock = matchTime + ":00",
                home_score = 0,
                away_score = 0,
                team = collection["payload.timeline.team"],
                period_type = PeriodType.RegularPeriod.DisplayName,
                status = collection["payload.timeline.status"],
                period = match.MatchResult?.Period ?? 1
            };
            if (match.MatchResult?.MatchPeriods != null)
            {
                foreach (var matchPeriod in match.MatchResult.MatchPeriods)
                {
                    if (matchPeriod.PeriodType.IsPenalties()) continue;
                    requestBody.payload.timeline.home_score += matchPeriod.HomeScore;
                    requestBody.payload.timeline.away_score += matchPeriod.AwayScore;
                }
            }

            requestBody.payload.sport_event_status.home_score = requestBody.payload.timeline.home_score;
            requestBody.payload.sport_event_status.away_score = requestBody.payload.timeline.away_score;
            requestBody.payload.sport_event_status.clock.match_time = requestBody.payload.timeline.match_clock;

            var commentary1 = collection["commentary1"];
            var commentary2 = collection["commentary2"];
            var commentary3 = collection["commentary3"];
            var commentaries = new List<CommentaryDto>();
            if (!string.IsNullOrWhiteSpace(commentary1))
            {
                commentaries.Add(new CommentaryDto() { text = commentary1 });
            }
            if (!string.IsNullOrWhiteSpace(commentary2))
            {
                commentaries.Add(new CommentaryDto() { text = commentary2 });
            }
            if (!string.IsNullOrWhiteSpace(commentary3))
            {
                commentaries.Add(new CommentaryDto() { text = commentary3 });
            }
            requestBody.payload.timeline.commentaries = commentaries;

            return requestBody;
        }

        public async Task CreateSubstitutionEventPartial(string id, IFormCollection collection, TimeSpan offset)
        {
            var requestBody = await InitEvent(id, collection, offset);
            requestBody.payload.timeline.player_in =
                new Player_In()
                {
                    id = UniqueGenerator.GetIdWithPattern("sr:player:"),
                    name = collection["payload.timeline.player_in.name"]
                };

            requestBody.payload.timeline.player_out =
                new Player_Out()
                {
                    id = UniqueGenerator.GetIdWithPattern("sr:player:"),
                    name = collection["payload.timeline.player_out.name"]
                };
            requestBody.payload.timeline.type = EventType.Substitution.DisplayName;

            await simulatorService.SendMatchEvent(requestBody);
            await timelineService.SendMatchTimeline(id);
        }

        public async Task CreateMinorEventPartial(string id, IFormCollection collection, TimeSpan offset)
        {
            var requestBody = await InitEvent(id, collection, offset);

            await simulatorService.SendMatchEvent(requestBody);
            await timelineService.SendMatchTimeline(id);
        }

        public async Task CreatePenaltyShootEventPartial(string id, IFormCollection collection, TimeSpan offset)
        {
            var requestBody = await InitEvent(id, collection, offset);
            var isNotDuplicateEvent = string.IsNullOrWhiteSpace(collection["payload.timeline.id"]);

            if (!string.IsNullOrWhiteSpace(collection["payload.timeline.goal_scorer.name"]))
            {
                requestBody.payload.timeline.player = new TimelinePlayer()
                {
                    id = UniqueGenerator.GetIdWithPattern("sr:player:"),
                    name = collection["payload.timeline.goal_scorer.name"]
                };
            }
            requestBody.payload.timeline.type = EventType.PenaltyShootout.DisplayName;
            requestBody.payload.timeline.period_type = PeriodType.Penalties.DisplayName;

            if (isNotDuplicateEvent)
            {
                requestBody = UpdatePenaltyScore(requestBody);
            }

            await simulatorService.SendMatchEvent(requestBody);
            await timelineService.SendMatchTimeline(id);
        }

        public async Task CreateScoreEventPartial(string id, IFormCollection collection, TimeSpan offset)
        {
            var requestBody = await InitEvent(id, collection, offset);
            var isNotDuplicateEvent = string.IsNullOrWhiteSpace(collection["payload.timeline.id"]);

            if (!string.IsNullOrWhiteSpace(collection["payload.timeline.assist.name"]))
            {
                requestBody.payload.timeline.assist = new Assist
                {
                    id = UniqueGenerator.GetIdWithPattern("sr:player:"),
                    name = collection["payload.timeline.assist.name"],
                    type = "primary"
                };
            }

            if (requestBody.payload.timeline.type == EventType.PenaltyMissed.DisplayName)
            {
                requestBody.payload.timeline.player = new TimelinePlayer()
                {
                    id = UniqueGenerator.GetIdWithPattern("sr:player:"),
                    name = collection["payload.timeline.goal_scorer.name"]
                };
            }
            else
            {
                requestBody.payload.timeline.goal_scorer = new Goal_Scorer
                {
                    id = UniqueGenerator.GetIdWithPattern("sr:player:"),
                    name = collection["payload.timeline.goal_scorer.name"]
                };
            }

            if (requestBody.payload.timeline.type != "normal_goal" && requestBody.payload.timeline.type != EventType.PenaltyMissed.DisplayName)
            {
                requestBody.payload.timeline.goal_scorer.method = requestBody.payload.timeline.type;
            }
            var periodType = collection["payload.timeline.period_type"];
            if (periodType == "extra_time")
            {
                requestBody.payload.timeline.period_type = "overtime";
                requestBody.payload.timeline.stoppage_time = collection["payload.timeline.stoppage_time"];
            }

            requestBody.payload.timeline.type = requestBody.payload.timeline.type != EventType.PenaltyMissed.DisplayName ?
                EventType.ScoreChange.DisplayName
                : EventType.PenaltyMissed.DisplayName;
            if (isNotDuplicateEvent && requestBody.payload.timeline.type != EventType.PenaltyMissed.DisplayName)
            {
                requestBody = await UpdateScoreChange(requestBody, periodType, id);
            }

            await simulatorService.SendMatchEvent(requestBody);
            await timelineService.SendMatchTimeline(id);

            await matchService.UpdateLiveMatch(id);
        }

        public async Task CreateTimeEventPartial(string id, IFormCollection collection, TimeSpan offset)
        {
            var matchRepository = new MatchRepository();
            var match = await matchRepository.GetMatchById(id);
            var requestBody = await InitEvent(id, collection, offset);

            requestBody.payload.sport_event_status.aggregate_away_score =
                collection.ContainsKey("aggregate_away_score")
                ? int.Parse(collection["aggregate_away_score"])
                : 0;
            requestBody.payload.sport_event_status.aggregate_home_score =
                collection.ContainsKey("aggregate_home_score")
                ? int.Parse(collection["aggregate_home_score"])
                : 0;
            switch (collection["aggregate_winner_id"])
            {
                case "home":
                    requestBody.payload.sport_event_status.aggregate_winner_id = match.Teams.FirstOrDefault(x => x.IsHome).Id;
                    break;

                case "away":
                    requestBody.payload.sport_event_status.aggregate_winner_id = match.Teams.FirstOrDefault(x => !x.IsHome).Id;
                    break;

                default:
                    requestBody.payload.sport_event_status.aggregate_winner_id = null;
                    requestBody.payload.sport_event_status.aggregate_away_score = 0;
                    requestBody.payload.sport_event_status.aggregate_home_score = 0;
                    break;
            }

            var eventType = collection["payload.timeline.type"];

            switch (eventType)
            {
                case "match_started":
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
                    break;

                case "1st_period_start":
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

                        break;
                    }
                case "break_start":
                    {
                        requestBody.payload.sport_event_status.status = "live";
                        requestBody.payload.sport_event_status.match_status = MatchStatus.Halftime.DisplayName;
                        requestBody.payload.sport_event_status.clock = new Clock() { match_time = "45:00" };
                        requestBody.payload.sport_event_status.period = 1;
                        requestBody.payload.timeline.period_type = null;
                        requestBody.payload.timeline.type = EventType.BreakStart.DisplayName;
                        requestBody.payload.timeline.match_clock = "45:00";
                        requestBody.payload.timeline.match_time = 45;
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

                        break;
                    }
                case "2nd_period_start":
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
                    break;

                case "await_extra":
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

                        break;
                    }
                case "1st_extra_start":
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

                        break;
                    }
                case "break_extra":
                    {
                        requestBody.payload.sport_event_status.status = "live";
                        requestBody.payload.sport_event_status.match_status = MatchStatus.ExtraTimeHaftTime.DisplayName;
                        requestBody.payload.sport_event_status.clock = new Clock() { match_time = "105:00" };
                        requestBody.payload.sport_event_status.period = 2;
                        requestBody.payload.timeline.period_type = null;
                        requestBody.payload.timeline.type = EventType.BreakStart.DisplayName;
                        requestBody.payload.timeline.period_name = "Extra time halftime";
                        requestBody.payload.timeline.match_clock = "105:00";
                        requestBody.payload.timeline.match_time = 105;
                        if (requestBody.payload.sport_event_status.period_scores.FirstOrDefault(x => x.type == PeriodType.Overtime.DisplayName) == null)
                        {
                            requestBody.payload.sport_event_status.period_scores = requestBody.payload.sport_event_status.period_scores.Append(new Period_Scores
                            {
                                home_score = 0,
                                away_score = 0,
                                type = "overtime"
                            });
                        }

                        break;
                    }
                case "2nd_extra_start":
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

                        break;
                    }
                case "await_penalty":
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

                        break;
                    }
                case "penalty_start":
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

                        break;
                    }
                case "match_end_normal":
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

                        break;
                    }
                case "match_end_extra":
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

                        break;
                    }
                case "match_end_pen":
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
                        requestBody.payload.sport_event_status.winner_id = penaltyResult.home_score > penaltyResult.away_score ?
                            match.Teams.FirstOrDefault(x => x.IsHome).Id
                            : match.Teams.FirstOrDefault(x => !x.IsHome).Id;

                        break;
                    }
                default:
                    return;
            }

            await simulatorService.SendMatchEvent(requestBody);
            await timelineService.SendMatchTimeline(id);

            await matchService.UpdateLiveMatch(match.Id);
        }

        private static async Task<MatchPushEventDto> UpdateScoreChange(MatchPushEventDto requestBody, string periodType, string matchId)
        {
            var matchRepository = new MatchRepository();
            var match = await matchRepository.GetMatchById(matchId);
            var isHomeScore = requestBody.payload.timeline.team == "home";

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

            return requestBody;
        }

        private static MatchPushEventDto UpdatePenaltyScore(MatchPushEventDto requestBody)
        {
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

            return requestBody;
        }
    }
}
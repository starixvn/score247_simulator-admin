using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Score247Management.Core._Shared.Enumerations;
using Score247Management.Helpers;
using Score247Management.Models.Events;
using Score247Management.Repositories.League;
using Score247Management.Repositories.Match;
using Score247Management.Repositories.Match.Models;
using Score247Management.Repositories.Team;
using Score247Management.ViewModels.Match;

namespace Score247Management.Controllers
{
    public class MatchController : Controller
    {
        //public const string RequestPrefix = "";
        public const string RequestPrefix = "http://localhost:61092";
        public const string EventPrefix = "/api/soccer-xt3/other/stream/events/PushEvent";
        // GET: Match
        public async Task<ActionResult> Index()
        {
            var repository = new MatchRepository();
            var matches = await repository.GetMatches();
            return View(matches);
        }

        // GET: Match/Details/5
        public async Task<ActionResult> Details(string id)
        {
            var repository = new MatchRepository();
            var match = await repository.GetMatchById(id);
            var timelines = await repository.GetMatchTimelines(id);
            var viewModel = new EventMatchViewModel();
            viewModel.Match = match;
            viewModel.TimelineEvents = timelines;
            return View(viewModel);
        }

        // GET: Match/Create
        public async Task<ActionResult> Create()
        {
            var leagueRepo = new LeagueRepository();
            var leagues = await leagueRepo.GetLeagues();

            var teamRepo = new TeamRepository();
            var teams = await teamRepo.GetTeams();

            var viewModel = new CreateMatchViewModel();
            viewModel.Tournaments = leagues;
            viewModel.Teams = teams;

            return View(viewModel);
        }

        // POST: Match/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(IFormCollection collection)
        {
            try
            {
                // TODO: Add insert logic here
                var leagueRepo = new LeagueRepository();
                var leagues = await leagueRepo.GetLeagues();

                var teamRepo = new TeamRepository();
                var teams = await teamRepo.GetTeams();

                var newMatch = new NewMatch();
                newMatch.Id = UniqueGenerator.GetIdWithPattern("sr:match:");
                newMatch.Status = collection["NewMatch.Status"];
                newMatch.Venue = new Core.Matches.Models.Venue
                {
                    Id = "sr:venue:" + collection["NewMatch.Venue.Name"],
                    Name = collection["NewMatch.Venue.Name"],
                    CountryName = collection["NewMatch.Venue.CountryName"]
                };
                newMatch.Tournament = leagues.FirstOrDefault(x => x.Id == collection["NewMatch.Tournament.Id"]);
                var competitors = new List<Core.Teams.Models.Team>();
                competitors.Add(teams.FirstOrDefault(x => x.Id == collection["HomeTeam.Id"]));
                competitors.Add(teams.FirstOrDefault(x => x.Id == collection["AwayTeam.Id"]));
                newMatch.Competitors = competitors;
                var dateTime = Convert.ToDateTime(collection["NewMatch.Scheduled"]);
                var timeSpan = TimeSpan.Parse(collection["NewMatch.ScheduledTime"]);
                newMatch.Scheduled = new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, timeSpan.Hours, timeSpan.Minutes, timeSpan.Seconds);
                newMatch.TournamentRound = new Core.Leagues.Models.LeagueRound();
                newMatch.TournamentRound.Type = LeagueRoundType.FromDisplayName<LeagueRoundType>(collection["NewMatch.TournamentRound.Type.Value"]);
                newMatch.TournamentRound.Name = collection["NewMatch.TournamentRound.Name"];
                newMatch.TournamentRound.Number = Int32.Parse(collection["NewMatch.TournamentRound.Number"]);
                newMatch.TournamentRound.Phase = collection["NewMatch.TournamentRound.Phase"];
                newMatch.TournamentRound.Group = collection["NewMatch.TournamentRound.Group"];
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(RequestPrefix);
                    var newMatches = new List<NewMatch> { newMatch };
                    var content = new Repositories.Match.Models.DailyScheduleDto();
                    content.GeneratedAt = DateTime.UtcNow;
                    content.Schema = "Matches";
                    content.sport_events = newMatches;
                    var response = await client.PostAsJsonAsync(string.Format("/DailySchedule?date={0}", newMatch.Scheduled.UtcDateTime.ToShortDateString()), content);
                    if (!response.IsSuccessStatusCode)
                    {
                        return View();
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: Match/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: Match/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, IFormCollection collection)
        {
            try
            {
                // TODO: Add update logic here

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: Match/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: Match/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                // TODO: Add delete logic here

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> _CreateTimeEventPartial(string id, IFormCollection collection)
        {
            try
            {
                var matchRepository = new MatchRepository();
                var match = await matchRepository.GetMatchById(id);
                var requestBody = new MatchPushEventDto();
                requestBody.metadata = new PushEventMetaData()
                {
                    season_id = "",
                    sport_event_id = id,
                    sport_id = "sr:sport:1",
                    tournament_id = match.League.Id
                };
                requestBody.payload = new PushEventPayload();
                requestBody.payload.sport_event_status = new Sport_Event_Status()
                {
                    aggregate_away_score = match.MatchResult.AggregateAwayScore,
                    aggregate_home_score = match.MatchResult.AggregateHomeScore,
                    aggregate_winner_id = match.MatchResult.AggregateWinnerId,
                    away_score = match.MatchResult.AwayScore,
                    clock = new Clock { match_time = "00:00" },
                    home_score = match.MatchResult.HomeScore,
                    match_status = match.MatchResult.MatchStatus.DisplayName,
                    period = match.MatchResult.Period,
                    period_scores = match.MatchResult.MatchPeriods?.Select(x => new Period_Scores { home_score = x.HomeScore, away_score = x.AwayScore, number = x.Number, type = x.PeriodType.DisplayName }),
                    status = "live",
                    winner_id = match.MatchResult.WinnerId
                };
                var eventDate = Convert.ToDateTime(collection["payload.timeline.time"]);
                var eventTimeSpan = TimeSpan.Parse(collection["payload.timeline.hour"]);
                var eventTime = new DateTime(eventDate.Year, eventDate.Month, eventDate.Day, eventTimeSpan.Hours, eventTimeSpan.Minutes, eventTimeSpan.Seconds);
                requestBody.payload.timeline = new TimelineDto()
                {
                    id = UniqueGenerator.GetIntId(),
                    time = eventTime
                };
                var eventType = collection["payload.timeline.type"];

                if (eventType == "match_started")
                {
                    requestBody.payload.sport_event_status.status = "live";
                    requestBody.payload.sport_event_status.match_status = MatchStatus.FirstHaft.DisplayName;
                    requestBody.payload.sport_event_status.period = 1;
                    requestBody.payload.timeline.type = EventType.MatchStarted.DisplayName;
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
                }
                else if (eventType == "break_start")
                {
                    requestBody.payload.sport_event_status.status = "live";
                    requestBody.payload.sport_event_status.match_status = MatchStatus.Halftime.DisplayName;
                    requestBody.payload.sport_event_status.period = 1;
                    requestBody.payload.timeline.type = EventType.BreakStart.DisplayName;
                    requestBody.payload.timeline.period_name = MatchStatus.Pause.DisplayName;
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
                }
                else if (eventType == "await_extra")
                {
                    requestBody.payload.sport_event_status.status = "live";
                    requestBody.payload.sport_event_status.match_status = MatchStatus.AwaitingExtraTime.DisplayName;
                    requestBody.payload.sport_event_status.period = 1;
                    requestBody.payload.timeline.type = EventType.BreakStart.DisplayName;
                    requestBody.payload.timeline.period_name = MatchStatus.AwaitingExtraTime.DisplayName;
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
                }
                else if (eventType == "break_extra")
                {
                    requestBody.payload.sport_event_status.status = "live";
                    requestBody.payload.sport_event_status.match_status = MatchStatus.SecondHaftExtra.DisplayName;
                    requestBody.payload.sport_event_status.period = 2;
                    requestBody.payload.timeline.type = EventType.BreakStart.DisplayName;
                    requestBody.payload.timeline.period_name = MatchStatus.ExtraTimeHaftTime.DisplayName;
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
                }
                else if (eventType == "await_penalty")
                {
                    requestBody.payload.sport_event_status.status = "live";
                    requestBody.payload.sport_event_status.match_status = MatchStatus.AwaitingPenalties.DisplayName;
                    requestBody.payload.sport_event_status.period = 1;
                    requestBody.payload.timeline.type = EventType.BreakStart.DisplayName;
                    requestBody.payload.timeline.period_name = MatchStatus.AwaitingPenalties.DisplayName;
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
                }
                else if (eventType == "match_end_normal")
                {
                    requestBody.payload.sport_event_status.status = "closed";
                    requestBody.payload.sport_event_status.match_status = MatchStatus.Ended.DisplayName;
                    requestBody.payload.timeline.type = EventType.MatchEnded.DisplayName;
                    requestBody.payload.timeline.match_clock = "90:00";
                }
                else if (eventType == "match_end_extra")
                {
                    requestBody.payload.sport_event_status.status = "closed";
                    requestBody.payload.sport_event_status.match_status = MatchStatus.EndedExtraTime.DisplayName;
                    requestBody.payload.timeline.type = EventType.MatchEnded.DisplayName;
                    requestBody.payload.timeline.match_clock = "120:00";
                }
                else if (eventType == "match_end_pen")
                {
                    requestBody.payload.sport_event_status.status = "closed";
                    requestBody.payload.sport_event_status.match_status = MatchStatus.EndedAfterPenalties.DisplayName;
                    requestBody.payload.sport_event_status.period = 1;
                    requestBody.payload.timeline.type = EventType.MatchEnded.DisplayName;
                    requestBody.payload.timeline.match_clock = "120:00";
                }
                else
                {
                    return View();
                }
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(RequestPrefix);
                    var response = await client.PostAsJsonAsync(EventPrefix, requestBody);
                    if (!response.IsSuccessStatusCode)
                    {
                        return RedirectToAction(nameof(Details), new { id = id });
                    }
                }
                return RedirectToAction(nameof(Details), new { id = id });
            }
            catch (Exception ex)
            {
                return RedirectToAction(nameof(Details), new { id = id });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> _CreateScoreEventPartial(string id, IFormCollection collection)
        {
            try
            {
                var matchRepository = new MatchRepository();
                var match = await matchRepository.GetMatchById(id);
                var requestBody = new MatchPushEventDto();
                requestBody.metadata = new PushEventMetaData()
                {
                    season_id = "",
                    sport_event_id = id,
                    sport_id = "sr:sport:1",
                    tournament_id = match.League.Id
                };
                requestBody.payload = new PushEventPayload();
                requestBody.payload.sport_event_status = new Sport_Event_Status()
                {
                    aggregate_away_score = match.MatchResult.AggregateAwayScore,
                    aggregate_home_score = match.MatchResult.AggregateHomeScore,
                    aggregate_winner_id = match.MatchResult.AggregateWinnerId,
                    away_score = match.MatchResult.AwayScore,
                    clock = new Clock { match_time = "00:00" },
                    home_score = match.MatchResult.HomeScore,
                    match_status = match.MatchResult.MatchStatus.DisplayName,
                    period = match.MatchResult.Period,
                    period_scores = match.MatchResult.MatchPeriods?.Select(x => new Period_Scores { home_score = x.HomeScore, away_score = x.AwayScore, number = x.Number, type = x.PeriodType.DisplayName }),
                    status = "live",
                    winner_id = match.MatchResult.WinnerId
                };
                var eventDate = Convert.ToDateTime(collection["payload.timeline.time"]);
                var eventTimeSpan = TimeSpan.Parse(collection["payload.timeline.hour"]);
                var eventTime = new DateTime(eventDate.Year, eventDate.Month, eventDate.Day, eventTimeSpan.Hours, eventTimeSpan.Minutes, eventTimeSpan.Seconds);
                requestBody.payload.timeline = new TimelineDto()
                {
                    id = UniqueGenerator.GetIntId(),
                    time = eventTime
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
                requestBody.payload.timeline.match_clock = collection["payload.timeline.match_clock"];
                requestBody.payload.timeline.home_score = match.LatestTimeline != null ? match.LatestTimeline.HomeScore: 0;
                requestBody.payload.timeline.away_score = match.LatestTimeline != null ? match.LatestTimeline.AwayScore: 0;
                if (requestBody.payload.timeline.team == "home")
                {
                    requestBody.payload.timeline.home_score++;
                }
                else
                {
                    requestBody.payload.timeline.away_score++;
                }

                var eventType = collection["payload.timeline.type"];
                if (eventType == "own_goal")
                {
                    requestBody.payload.timeline.goal_scorer.method = "own_goal";
                }
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(RequestPrefix);
                    var response = await client.PostAsJsonAsync(EventPrefix, requestBody);
                    if (!response.IsSuccessStatusCode)
                    {
                        return RedirectToAction(nameof(Details), new { id = id });
                    }
                }
                return RedirectToAction(nameof(Details), new { id = id });
            }
            catch (Exception ex)
            {
                return RedirectToAction(nameof(Details), new { id = id });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> _CreatePenaltyShootEventPartial(string id, IFormCollection collection)
        {
            try
            {
                var matchRepository = new MatchRepository();
                var match = await matchRepository.GetMatchById(id);
                var requestBody = new MatchPushEventDto();
                requestBody.metadata = new PushEventMetaData()
                {
                    season_id = "",
                    sport_event_id = id,
                    sport_id = "sr:sport:1",
                    tournament_id = match.League.Id
                };
                requestBody.payload = new PushEventPayload();
                requestBody.payload.sport_event_status = new Sport_Event_Status()
                {
                    aggregate_away_score = match.MatchResult.AggregateAwayScore,
                    aggregate_home_score = match.MatchResult.AggregateHomeScore,
                    aggregate_winner_id = match.MatchResult.AggregateWinnerId,
                    away_score = match.MatchResult.AwayScore,
                    clock = new Clock { match_time = "00:00" },
                    home_score = match.MatchResult.HomeScore,
                    match_status = match.MatchResult.MatchStatus.DisplayName,
                    period = match.MatchResult.Period,
                    period_scores = match.MatchResult.MatchPeriods?.Select(x => new Period_Scores { home_score = x.HomeScore, away_score = x.AwayScore, number = x.Number, type = x.PeriodType.DisplayName }),
                    status = "live",
                    winner_id = match.MatchResult.WinnerId
                };
                var eventDate = Convert.ToDateTime(collection["payload.timeline.time"]);
                var eventTimeSpan = TimeSpan.Parse(collection["payload.timeline.hour"]);
                var eventTime = new DateTime(eventDate.Year, eventDate.Month, eventDate.Day, eventTimeSpan.Hours, eventTimeSpan.Minutes, eventTimeSpan.Seconds);
                requestBody.payload.timeline = new TimelineDto()
                {
                    id = UniqueGenerator.GetIntId(),
                    time = eventTime
                };

                if (!string.IsNullOrWhiteSpace(collection["payload.timeline.goal_scorer.name"]))
                {
                    requestBody.payload.timeline.goal_scorer = new Goal_Scorer()
                    {
                        id = UniqueGenerator.GetIdWithPattern("sr:player:"),
                        name = collection["payload.timeline.goal_scorer.name"]
                    };
                }
                requestBody.payload.timeline.type = EventType.PenaltyShootout.DisplayName;
                requestBody.payload.timeline.team = collection["payload.timeline.team"];
                requestBody.payload.timeline.period_type = PeriodType.Penalties.DisplayName;
                requestBody.payload.timeline.status = collection["payload.timeline.status"];

                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(RequestPrefix);
                    var response = await client.PostAsJsonAsync(EventPrefix, requestBody);
                    if (!response.IsSuccessStatusCode)
                    {
                        return RedirectToAction(nameof(Details), new { id = id });
                    }
                }
                return RedirectToAction(nameof(Details), new { id = id });
            }
            catch (Exception ex)
            {
                return RedirectToAction(nameof(Details), new { id = id });
            }
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> CreatCardFaultEventPartial(string id, IFormCollection collection)
        {
            try
            {
                var matchRepository = new MatchRepository();
                var match = await matchRepository.GetMatchById(id);
                var requestBody = new MatchPushEventDto();
                requestBody.metadata = new PushEventMetaData()
                {
                    season_id = "",
                    sport_event_id = id,
                    sport_id = "sr:sport:1",
                    tournament_id = match.League.Id
                };
                requestBody.payload = new PushEventPayload();
                requestBody.payload.sport_event_status = new Sport_Event_Status()
                {
                    aggregate_away_score = match.MatchResult.AggregateAwayScore,
                    aggregate_home_score = match.MatchResult.AggregateHomeScore,
                    aggregate_winner_id = match.MatchResult.AggregateWinnerId,
                    away_score = match.MatchResult.AwayScore,
                    clock = new Clock { match_time = "00:00" },
                    home_score = match.MatchResult.HomeScore,
                    match_status = match.MatchResult.MatchStatus.DisplayName,
                    period = match.MatchResult.Period,
                    period_scores = match.MatchResult.MatchPeriods?.Select(x => new Period_Scores { home_score = x.HomeScore, away_score = x.AwayScore, number = x.Number, type = x.PeriodType.DisplayName }),
                    status = "live",
                    winner_id = match.MatchResult.WinnerId
                };
                var eventDate = Convert.ToDateTime(collection["payload.timeline.time"]);
                var eventTimeSpan = TimeSpan.Parse(collection["payload.timeline.hour"]);
                var eventTime = new DateTime(eventDate.Year, eventDate.Month, eventDate.Day, eventTimeSpan.Hours, eventTimeSpan.Minutes, eventTimeSpan.Seconds);
                requestBody.payload.timeline = new TimelineDto()
                {
                    id = UniqueGenerator.GetIntId(),
                    time = eventTime,
                    period = match.MatchResult.Period,
                    type = collection["payload.timeline.type"],
                    team = collection["payload.timeline.team"],
                    match_time = Int32.Parse(collection["payload.timeline.match_time"]),
                    match_clock = collection["payload.timeline.match_clock"]
                };

                if (!string.IsNullOrWhiteSpace(collection["payload.timeline.player.name"]))
                {
                    requestBody.payload.timeline.player = new TimelinePlayer()
                    {
                        id = UniqueGenerator.GetIdWithPattern("sr:player:"),
                        name = collection["payload.timeline.player.name"]
                    };
                }
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(RequestPrefix);
                    var response = await client.PostAsJsonAsync(EventPrefix, requestBody);
                    if (!response.IsSuccessStatusCode)
                    {
                        return RedirectToAction(nameof(Details), new { id = id });
                    }
                }
                return RedirectToAction(nameof(Details), new { id = id });
            }
            catch (Exception ex)
            {
                return RedirectToAction(nameof(Details), new { id = id });
            }
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> CreateInjuryExtraTimeEventPartial(string id, IFormCollection collection)
        {
            try
            {
                var matchRepository = new MatchRepository();
                var match = await matchRepository.GetMatchById(id);
                var requestBody = new MatchPushEventDto();
                requestBody.metadata = new PushEventMetaData()
                {
                    season_id = "",
                    sport_event_id = id,
                    sport_id = "sr:sport:1",
                    tournament_id = match.League.Id
                };
                requestBody.payload = new PushEventPayload();
                requestBody.payload.sport_event_status = new Sport_Event_Status()
                {
                    aggregate_away_score = match.MatchResult.AggregateAwayScore,
                    aggregate_home_score = match.MatchResult.AggregateHomeScore,
                    aggregate_winner_id = match.MatchResult.AggregateWinnerId,
                    away_score = match.MatchResult.AwayScore,
                    clock = new Clock { match_time = "00:00" },
                    home_score = match.MatchResult.HomeScore,
                    match_status = match.MatchResult.MatchStatus.DisplayName,
                    period = match.MatchResult.Period,
                    period_scores = match.MatchResult.MatchPeriods?.Select(x => new Period_Scores { home_score = x.HomeScore, away_score = x.AwayScore, number = x.Number, type = x.PeriodType.DisplayName }),
                    status = "live",
                    winner_id = match.MatchResult.WinnerId
                };
                var eventDate = Convert.ToDateTime(collection["payload.timeline.time"]);
                var eventTimeSpan = TimeSpan.Parse(collection["payload.timeline.hour"]);
                var eventTime = new DateTime(eventDate.Year, eventDate.Month, eventDate.Day, eventTimeSpan.Hours, eventTimeSpan.Minutes, eventTimeSpan.Seconds);
                requestBody.payload.timeline = new TimelineDto()
                {
                    id = UniqueGenerator.GetIntId(),
                    time = eventTime,
                    period = match.MatchResult.Period,
                    type = EventType.InjuryTimeShown.DisplayName,
                    match_time = Int32.Parse(collection["payload.timeline.match_time"]),
                    match_clock = collection["payload.timeline.match_clock"],
                    injury_time_announced = Int32.Parse(collection["payload.timeline.injury_time_announced"])
                };
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(RequestPrefix);
                    var response = await client.PostAsJsonAsync(EventPrefix, requestBody);
                    if (!response.IsSuccessStatusCode)
                    {
                        return RedirectToAction(nameof(Details), new { id = id });
                    }
                }
                return RedirectToAction(nameof(Details), new { id = id });
            }
            catch (Exception ex)
            {
                return RedirectToAction(nameof(Details), new { id = id });
            }
        }
    }
}
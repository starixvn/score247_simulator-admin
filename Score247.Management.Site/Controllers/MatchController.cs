using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Score247Management.Core._Shared.Enumerations;
using Score247.Management.Site.Helpers;
using Score247.Management.Site.Models.Events;
using Score247.Management.Site.Repositories.League;
using Score247.Management.Site.Repositories.Match;
using Score247.Management.Site.Repositories.Match.Models;
using Score247.Management.Site.Repositories.Team;
using Score247.Management.Site.ViewModels.Match;
using Newtonsoft.Json;
using System.Text;
using Microsoft.Extensions.Configuration;
using Score247.Management.Site.Models;
using Score247.Management.Site.Services.Matchs;

namespace Score247.Management.Site.Controllers
{
    public class MatchController : Controller
    {
        private IConfiguration Configuration { get; set; }

        public MatchController(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        // GET: Match
        public async Task<ActionResult> Index()
        {
            var repository = new MatchRepository();
            var leagueRepository = new LeagueRepository();
            var leagues = await leagueRepository.GetLeagues();
            var viewModal = new MatchViewModel();
            var timeNow = DateTime.Now;
            var fromDate = new DateTime(timeNow.Year, timeNow.Month, timeNow.Day, 0, 0, 0);
            var toDate = new DateTime(timeNow.Year, timeNow.Month, timeNow.Day, 23, 59, 59);
            viewModal.DateSearchModel = new DateSearchModel()
            {
                FromDate = fromDate,
                ToDate = toDate,
                Leagues = leagues
            };
            var matches = await repository.GetMatchByDataRange(viewModal.DateSearchModel);
            viewModal.Matches = matches;
            return View(viewModal);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> _DateRangeSearch(IFormCollection collection)
        {
            var fromDate = DateTime.Parse(collection["FromDate"]);
            fromDate = new DateTime(fromDate.Year, fromDate.Month, fromDate.Day, 0, 0, 0);
            var toDate = DateTime.Parse(collection["ToDate"]);
            toDate = new DateTime(toDate.Year, toDate.Month, toDate.Day, 23, 59, 59);
            var selectedLeagueId = collection["SelectedLeagueId"];
            var repository = new MatchRepository();
            var leagueRepository = new LeagueRepository();
            var leagues = await leagueRepository.GetLeagues();
            var viewModal = new MatchViewModel();
            viewModal.DateSearchModel = new DateSearchModel()
            {
                FromDate = fromDate,
                ToDate = toDate,
                Leagues = leagues,
                SelectedLeagueId = selectedLeagueId
            };
            var matches = await repository.GetMatchByDataRange(viewModal.DateSearchModel);
            viewModal.Matches = matches;
            return View("Index", viewModal);
        }


        // GET: Match/Details/5
        public async Task<ActionResult> Details(string id)
        {
            var ran = new Random();
            var repository = new MatchRepository();
            var match = await repository.GetMatchById(id);
            var timelines = await repository.GetMatchTimelines(id);
            var viewModel = new EventMatchViewModel();
            viewModel.Match = match;
            viewModel.TimelineEvents = timelines;
            viewModel.MatchCardEventDto = new MatchPushEventDto(match);
            viewModel.MatchCardEventDto.payload.timeline.player = new TimelinePlayer
            {
                name = "Player_" + ran.Next(909090).ToString()
            };
            viewModel.MatchGoalEventDto = new MatchPushEventDto(match);
            viewModel.MatchGoalEventDto.payload.timeline.goal_scorer = new Goal_Scorer
            {
                name = "Player_" + ran.Next(909090).ToString()
            };
            viewModel.MatchInjuryExtraEventDto = new MatchPushEventDto(match);
            viewModel.MatchPenaltyEventDto = new MatchPushEventDto(match);
            viewModel.MatchPenaltyEventDto.payload.timeline.goal_scorer = new Goal_Scorer
            {
                name = "Player_" + ran.Next(909090).ToString()
            };
            viewModel.MatchTimeEventDto = new MatchPushEventDto(match);
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
            viewModel.NewMatch = new NewMatch();
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
                newMatch.Venue = new Score247Management.Core.Matches.Models.Venue
                {
                    Id = "sr:venue:" + collection["NewMatch.Venue.Name"],
                    Name = collection["NewMatch.Venue.Name"],
                    CountryName = collection["NewMatch.Venue.CountryName"]
                };
                newMatch.Tournament = leagues.FirstOrDefault(x => x.Id == collection["NewMatch.Tournament.Id"]);
                var competitors = new List<Score247Management.Core.Teams.Models.Team>();
                var homeTeam = teams.FirstOrDefault(x => x.Id == collection["HomeTeam.Id"]);
                homeTeam.Statistic = new Score247Management.Core.Teams.Models.TeamStatistics();
                homeTeam.Qualifier = "home";
                var awayTeam = teams.FirstOrDefault(x => x.Id == collection["AwayTeam.Id"]);
                awayTeam.Statistic = new Score247Management.Core.Teams.Models.TeamStatistics();
                awayTeam.Qualifier = "away";
                competitors.Add(homeTeam);
                competitors.Add(awayTeam);
                newMatch.Competitors = competitors;
                var dateTime = Convert.ToDateTime(collection["NewMatch.Scheduled"]);
                var timeSpan = TimeSpan.Parse(collection["NewMatch.ScheduledTime"]);
                newMatch.Scheduled = new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, timeSpan.Hours, timeSpan.Minutes, timeSpan.Seconds, DateTimeKind.Utc);
                newMatch.TournamentRound = new Score247Management.Core.Leagues.Models.LeagueRound();
                newMatch.TournamentRound.Type = LeagueRoundType.FromDisplayName<LeagueRoundType>(collection["NewMatch.TournamentRound.Type.Value"]);
                newMatch.TournamentRound.Name = collection["NewMatch.TournamentRound.Name"];
                newMatch.TournamentRound.Number = Int32.Parse(collection["NewMatch.TournamentRound.Number"]);
                newMatch.TournamentRound.Phase = collection["NewMatch.TournamentRound.Phase"];
                newMatch.TournamentRound.Group = collection["NewMatch.TournamentRound.Group"];
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(Configuration.GetValue<string>("Simulator:Url"));
                    var newMatches = new List<NewMatch> { newMatch };
                    var content = new Repositories.Match.Models.DailyScheduleDto();
                    content.GeneratedAt = DateTime.UtcNow;
                    content.Schema = "Matches";
                    content.sport_events = newMatches;
                    var jsonValue = JsonConvert.SerializeObject(content);

                    var response = await client.PostAsync(string.Format(Configuration.GetValue<string>("Simulator:DailySchedule") + "?date={0}", newMatch.Scheduled.ToString("yyyy-MM-dd")), new StringContent(jsonValue, Encoding.UTF8, "application/json"));
                    if (!response.IsSuccessStatusCode)
                    {
                        return View();
                    }
                    var triggerClient = new HttpClient();
                    triggerClient.BaseAddress = new Uri(Configuration.GetValue<string>("Simulator:FetchUrl"));
                    var request = new HttpRequestMessage(HttpMethod.Post, "");
                    FormUrlEncodedContent fetchContent = new FormUrlEncodedContent(
                        new[]{
                            new KeyValuePair < string, string > ("jobs[]", "FetchPreMatch"),
                            new KeyValuePair < string, string > ("jobs[]", "FetchPostMatch"),
                            new KeyValuePair < string, string > ("jobs[]", "FetchLiveMatch")
                    });
                    request.Content = fetchContent;
                    triggerClient.SendAsync(request);

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
                var requestBody = new MatchPushEventDto(match);
                var eventDate = Convert.ToDateTime(collection["payload.timeline.time"]);
                var eventTimeSpan = TimeSpan.Parse(collection["payload.timeline.hour"]);
                var eventTime = new DateTime(eventDate.Year, eventDate.Month, eventDate.Day, eventTimeSpan.Hours, eventTimeSpan.Minutes, eventTimeSpan.Seconds, DateTimeKind.Utc);
                requestBody.payload.timeline = new TimelineDto()
                {
                    id = UniqueGenerator.GetIntId(),
                    time = eventTime
                };
                requestBody.payload.timeline.home_score = match.LatestTimeline != null ? match.LatestTimeline.HomeScore : 0;
                requestBody.payload.timeline.away_score = match.LatestTimeline != null ? match.LatestTimeline.AwayScore : 0;

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
                    if(requestBody.payload.sport_event_status.period_scores?.First(x => x.number == 1 && x.type == PeriodType.RegularPeriod.DisplayName) == null)
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
                    requestBody.payload.timeline.type = EventType.BreakStart.DisplayName;
                    requestBody.payload.timeline.period_name = MatchStatus.Pause.DisplayName;
                    if (requestBody.payload.sport_event_status.period_scores?.First(x => x.number == 1 && x.type == PeriodType.RegularPeriod.DisplayName) == null)
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
                    requestBody.payload.timeline.type = EventType.BreakStart.DisplayName;
                    requestBody.payload.timeline.period_name = "Awaiting extra";
                    if (requestBody.payload.sport_event_status.period_scores?.First(x => x.type == PeriodType.Overtime.DisplayName) == null)
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
                    if (requestBody.payload.sport_event_status.period_scores?.First(x => x.type == PeriodType.Overtime.DisplayName) == null)
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
                    requestBody.payload.timeline.type = EventType.BreakStart.DisplayName;
                    requestBody.payload.timeline.period_name = "Extra time halftime";
                    if (requestBody.payload.sport_event_status.period_scores?.First(x => x.type == PeriodType.Overtime.DisplayName) == null)
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
                    if (requestBody.payload.sport_event_status.period_scores?.First(x => x.type == PeriodType.Overtime.DisplayName) == null)
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
                    requestBody.payload.timeline.type = EventType.BreakStart.DisplayName;
                    requestBody.payload.timeline.period_name = MatchStatus.AwaitingPenalties.DisplayName;
                    if (requestBody.payload.sport_event_status.period_scores?.First(x => x.type == PeriodType.Penalties.DisplayName) == null)
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
                    if (requestBody.payload.sport_event_status.period_scores?.First(x => x.type == PeriodType.Penalties.DisplayName) == null)
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
                    if (requestBody.payload.sport_event_status.period_scores?.First(x => x.number == 1 && x.type == PeriodType.RegularPeriod.DisplayName) == null)
                    {
                        requestBody.payload.sport_event_status.period_scores = requestBody.payload.sport_event_status.period_scores.Append(new Period_Scores
                        {
                            home_score = 0,
                            away_score = 0,
                            type = "regular_period",
                            number = 1
                        });
                    }
                    if (requestBody.payload.sport_event_status.period_scores?.First(x => x.number == 2 && x.type == PeriodType.RegularPeriod.DisplayName) == null)
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
                    if (requestBody.payload.sport_event_status.period_scores?.First(x => x.type == PeriodType.Overtime.DisplayName) == null)
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
                    requestBody.payload.timeline.type = EventType.MatchEnded.DisplayName;
                    requestBody.payload.timeline.match_clock = "120:00";
                    if (requestBody.payload.sport_event_status.period_scores?.First(x => x.type == PeriodType.Penalties.DisplayName) == null)
                    {
                        requestBody.payload.sport_event_status.period_scores = requestBody.payload.sport_event_status.period_scores.Append(new Period_Scores
                        {
                            home_score = 0,
                            away_score = 0,
                            type = "penalties"
                        });
                    }
                }
                else
                {
                    return View();
                }
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(Configuration.GetValue<string>("Simulator:Url"));
                    var response = await client.PostAsJsonAsync(Configuration.GetValue<string>("Simulator:PushEvent"), requestBody);
                    if (!response.IsSuccessStatusCode)
                    {
                        return RedirectToAction(nameof(Details), new { id = id });
                    }
                }
                await matchRepository.InsertTimelineLog(requestBody);
                await Task.Delay(2000);
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
                var requestBody = new MatchPushEventDto(match);

                var eventDate = Convert.ToDateTime(collection["payload.timeline.time"]);
                var eventTimeSpan = TimeSpan.Parse(collection["payload.timeline.hour"]);
                var eventTime = new DateTime(eventDate.Year, eventDate.Month, eventDate.Day, eventTimeSpan.Hours, eventTimeSpan.Minutes, eventTimeSpan.Seconds, DateTimeKind.Utc);
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

                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(Configuration.GetValue<string>("Simulator:Url"));
                    var response = await client.PostAsJsonAsync(Configuration.GetValue<string>("Simulator:PushEvent"), requestBody);
                    if (!response.IsSuccessStatusCode)
                    {
                        return RedirectToAction(nameof(Details), new { id = id });
                    }
                }
                await matchRepository.InsertTimelineLog(requestBody);
                await Task.Delay(2000);
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
                var requestBody = new MatchPushEventDto(match);
                var eventDate = Convert.ToDateTime(collection["payload.timeline.time"]);
                var eventTimeSpan = TimeSpan.Parse(collection["payload.timeline.hour"]);
                var eventTime = new DateTime(eventDate.Year, eventDate.Month, eventDate.Day, eventTimeSpan.Hours, eventTimeSpan.Minutes, eventTimeSpan.Seconds, DateTimeKind.Utc);
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

                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(Configuration.GetValue<string>("Simulator:Url"));
                    var response = await client.PostAsJsonAsync(Configuration.GetValue<string>("Simulator:PushEvent"), requestBody);
                    if (!response.IsSuccessStatusCode)
                    {
                        return RedirectToAction(nameof(Details), new { id = id });
                    }
                }
                await matchRepository.InsertTimelineLog(requestBody);
                await Task.Delay(2000);
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
                var requestBody = new MatchPushEventDto(match);
                var eventDate = Convert.ToDateTime(collection["payload.timeline.time"]);
                var eventTimeSpan = TimeSpan.Parse(collection["payload.timeline.hour"]);
                var eventTime = new DateTime(eventDate.Year, eventDate.Month, eventDate.Day, eventTimeSpan.Hours, eventTimeSpan.Minutes, eventTimeSpan.Seconds, DateTimeKind.Utc);
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
                    away_score = match.LatestTimeline != null ? match.LatestTimeline.AwayScore : 0
                };


                if (collection["payload.timeline.period_type"] == "overtime")
                {
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
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(Configuration.GetValue<string>("Simulator:Url"));
                    var response = await client.PostAsJsonAsync(Configuration.GetValue<string>("Simulator:PushEvent"), requestBody);
                    if (!response.IsSuccessStatusCode)
                    {
                        return RedirectToAction(nameof(Details), new { id = id });
                    }
                }
                await matchRepository.InsertTimelineLog(requestBody);
                await Task.Delay(2000);
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
                var requestBody = new MatchPushEventDto(match);

                requestBody.payload.sport_event_status.match_status = collection["payload.sport_event_status.match_status"];

                var eventDate = Convert.ToDateTime(collection["payload.timeline.time"]);
                var eventTimeSpan = TimeSpan.Parse(collection["payload.timeline.hour"]);
                var eventTime = new DateTime(eventDate.Year, eventDate.Month, eventDate.Day, eventTimeSpan.Hours, eventTimeSpan.Minutes, eventTimeSpan.Seconds, DateTimeKind.Utc);
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
                };

                if (requestBody.payload.sport_event_status.match_status == MatchStatus.SecondHaft.DisplayName)
                {
                    requestBody.payload.timeline.period = 2;
                    requestBody.payload.timeline.match_time = 90;
                    requestBody.payload.timeline.match_clock = "90:00";
                }

                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(Configuration.GetValue<string>("Simulator:Url"));
                    var response = await client.PostAsJsonAsync(Configuration.GetValue<string>("Simulator:PushEvent"), requestBody);
                    if (!response.IsSuccessStatusCode)
                    {
                        return RedirectToAction(nameof(Details), new { id = id });
                    }
                }
                await matchRepository.InsertTimelineLog(requestBody);
                await Task.Delay(2000);
                return RedirectToAction(nameof(Details), new { id = id });
            }
            catch (Exception ex)
            {
                return RedirectToAction(nameof(Details), new { id = id });
            }
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AutoEventNormalMatch(string id, IFormCollection collection)
        {
            try
            {
                AutoEventService.PushNormalMatchEvents(id, Configuration.GetValue<string>("Simulator:Url"), Configuration.GetValue<string>("Simulator:PushEvent"));
                return RedirectToAction(nameof(Details), new { id = id });
            }
            catch (Exception ex)
            {
                return RedirectToAction(nameof(Details), new { id = id });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AutoEventOvertimeMatch(string id, IFormCollection collection)
        {
            try
            {
                AutoEventService.PushOvertimeMatchEvents(id, Configuration.GetValue<string>("Simulator:Url"), Configuration.GetValue<string>("Simulator:PushEvent"));
                return RedirectToAction(nameof(Details), new { id = id });
            }
            catch (Exception ex)
            {
                return RedirectToAction(nameof(Details), new { id = id });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AutoEventPenaltyMatch(string id, IFormCollection collection)
        {
            try
            {
                AutoEventService.PushPenaltyMatchEvents(id, Configuration.GetValue<string>("Simulator:Url"), Configuration.GetValue<string>("Simulator:PushEvent"));
                return RedirectToAction(nameof(Details), new { id = id });
            }
            catch (Exception ex)
            {
                return RedirectToAction(nameof(Details), new { id = id });
            }
        }

    }
}
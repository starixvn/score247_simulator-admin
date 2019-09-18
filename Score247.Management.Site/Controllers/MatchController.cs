using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using Score247.Management.Site.Criterias.Scenarios;
using Score247.Management.Site.Helpers;
using Score247.Management.Site.Models;
using Score247.Management.Site.Models.Events;
using Score247.Management.Site.Repositories.Leagues;
using Score247.Management.Site.Repositories.Matches;
using Score247.Management.Site.Repositories.Matches.Models;
using Score247.Management.Site.Repositories.Scenarios;
using Score247.Management.Site.Repositories.Teams;
using Score247.Management.Site.Services;
using Score247.Management.Site.Services.Matchs;
using Score247.Management.Site.ViewModels.Matches;
using Score247Management.Core._Shared.Enumerations;
using Score247Management.Core.Matches.Models;

namespace Score247.Management.Site.Controllers
{
    public class MatchController : Controller
    {
        private IConfiguration Configuration { get; set; }
        private CreateMatchesService CreateMatchesService { get; set; }

        public MatchController(IConfiguration configuration)
        {
            Configuration = configuration;
            CreateMatchesService = new CreateMatchesServiceImpl();
        }

        // GET: Match
        public async Task<ActionResult> Index()
        {
            var repository = new MatchRepository();
            var leagueRepository = new LeagueRepository();
            var leagues = await leagueRepository.GetLeagues();
            TimeSpan.TryParse(GetTimezone(), out TimeSpan offset);
            var viewModal = new MatchViewModel();
            var timeNow = DateTime.UtcNow.Add(offset);
            var fromDate = new DateTime(timeNow.Year, timeNow.Month, timeNow.Day, 0, 0, 0, DateTimeKind.Utc).Subtract(offset);
            var toDate = new DateTime(timeNow.Year, timeNow.Month, timeNow.Day, 23, 59, 59, DateTimeKind.Utc).Subtract(offset);
            viewModal.DateSearchModel = new DateSearchModel()
            {
                SearchDate = timeNow,
                Leagues = leagues,
                SelectedLeagueId = "all"
            };
            var matches = await repository.GetMatchByDataRangeWithOffset(fromDate, toDate, "all", offset);
            viewModal.Matches = matches;
            viewModal.CreateDeleteMatchesModels = CreateDeleteMatchesModel.CreateFromToDay();
            viewModal.QuickDateSelectOptions = GenerateQuickDateSelectOptions(offset);
            return View(viewModal);
        }

        private List<QuickDateSelectOption> GenerateQuickDateSelectOptions(TimeSpan offset)
        {
            var options = new List<QuickDateSelectOption>();
            var today = DateTime.UtcNow;
            options.Add(new QuickDateSelectOption() { DisplayName = "Live", Value = today.Add(offset).ToString("MM/dd/yyyy") });
            options.Add(new QuickDateSelectOption(today.AddDays(-2).Add(offset)));
            options.Add(new QuickDateSelectOption(today.AddDays(-1).Add(offset)));
            options.Add(new QuickDateSelectOption() { DisplayName = "Today, " + today.Add(offset).ToString("MMMM dd"), Value = today.Add(offset).ToString("MM/dd/yyyy") });
            options.Add(new QuickDateSelectOption(today.AddDays(1).Add(offset)));
            options.Add(new QuickDateSelectOption(today.AddDays(2).Add(offset)));

            return options;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> _DateRangeSearch(IFormCollection collection)
        {
            var eventDate = DateTime.Parse(collection["SearchDate"]);
            var selectedLeagueId = collection["SelectedLeagueId"];
            return await ProcessToDateSearchView(eventDate, selectedLeagueId);
        }

        public async Task<ActionResult> ShortcutSearchByDate(string searchDate, string leagueId)
        {
            return await ProcessToDateSearchView(DateTime.ParseExact(searchDate, "MM/dd/yyyy", null), leagueId);
        }

        private async Task<IEnumerable<Match>> SearchMatchByDateAndLeague(DateTime dateSearch, string leagueId, TimeSpan offset)
        {
            var repository = new MatchRepository();
            var fromDate = new DateTime(dateSearch.Year, dateSearch.Month, dateSearch.Day, 0, 0, 0, DateTimeKind.Utc).Subtract(offset);
            var toDate = new DateTime(dateSearch.Year, dateSearch.Month, dateSearch.Day, 23, 59, 59, DateTimeKind.Utc).Subtract(offset);

            return await repository.GetMatchByDataRangeWithOffset(fromDate, toDate, leagueId, offset);
        }

        private async Task<ActionResult> ProcessToDateSearchView(DateTime searchDate, string selectedLeagueId)
        {
            TimeSpan.TryParse(GetTimezone(), out TimeSpan offset);
            var leagueRepository = new LeagueRepository();
            var leagues = await leagueRepository.GetLeagues();
            var viewModal = new MatchViewModel
            {
                DateSearchModel = new DateSearchModel()
                {
                    SearchDate = searchDate,
                    Leagues = leagues,
                    SelectedLeagueId = selectedLeagueId
                }
            };

            var matches = await SearchMatchByDateAndLeague(searchDate, selectedLeagueId, offset);
            viewModal.Matches = matches;
            viewModal.CreateDeleteMatchesModels = CreateDeleteMatchesModel.CreateFromToDay();
            viewModal.QuickDateSelectOptions = GenerateQuickDateSelectOptions(offset);
            return View("Index", viewModal);
        }

        // GET: Match/Details/5
        public async Task<ActionResult> Details(string id)
        {
            var ran = new Random();
            var repository = new MatchRepository();
            var scenarioRepo = new ScenarioRepository();

            GetTimezone();
            TimeSpan offset;
            TimeSpan.TryParse(GetTimezone(), out offset);
            var match = await repository.GetMatchById(id, offset);
            var timelines = await repository.GetMatchTimelines(id, offset);
            var viewModel = new EventMatchViewModel();
            viewModel.Match = match;
            viewModel.TimelineEvents = timelines;
            viewModel.MatchCardEventDto = new MatchPushEventDto(match, offset);
            viewModel.MatchCardEventDto.payload.timeline.player = new TimelinePlayer
            {
                name = "Player_" + ran.Next(909090).ToString()
            };
            viewModel.MatchGoalEventDto = new MatchPushEventDto(match, offset);
            viewModel.MatchGoalEventDto.payload.timeline.goal_scorer = new Goal_Scorer
            {
                name = "Player_" + ran.Next(909090).ToString()
            };
            viewModel.MatchInjuryExtraEventDto = new MatchPushEventDto(match, offset);
            viewModel.MatchPenaltyEventDto = new MatchPushEventDto(match, offset);
            viewModel.MatchPenaltyEventDto.payload.timeline.goal_scorer = new Goal_Scorer
            {
                name = "Player_" + ran.Next(909090).ToString()
            };
            viewModel.MatchTimeEventDto = new MatchPushEventDto(match, offset);
            viewModel.Scenarios = await scenarioRepo.GetScenarios();
            return View(viewModel);
        }

        public async Task<ActionResult> Odds(string id)
        {
            var repository = new MatchRepository();

            TimeSpan offset;
            TimeSpan.TryParse(GetTimezone(), out offset);
            var match = await repository.GetMatchById(id, offset);
            var viewModel = new OddsEventViewModel();
            viewModel.Match = match;

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
            viewModel.Match = new MatchEntity();
            return View(viewModel);
        }

        // POST: Match/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(IFormCollection collection)
        {
            try
            {
                TimeSpan.TryParse(GetTimezone(), out TimeSpan offset);
                var matchId = await CreateMatchesService.CreateMatcheWithForm(collection, offset, Configuration);
                if (string.IsNullOrEmpty(matchId))
                {
                    return View();
                }
                return RedirectToAction(nameof(Details), new { id = matchId, timeSpan = collection["Timezone"].ToString() });
            }
            catch (Exception)
            {
                return View();
            }
        }

        [HttpGet("/Match/AutoCreateMatch")]
        public async Task<JsonResult> AutoCreateMatch(DateTimeOffset? dateTimeOffset)
            => Json(new
            {
                matchId = await CreateMatchesService.AutoCreateMatch(dateTimeOffset, Configuration)
            });

        [HttpPost("/Match/CreateMatchesByDate")]
        public async Task<JsonResult> CreateMatchesByDate([FromBody] CreateDeleteMatchesModel createDeleteMatchesRequest)
        {
            TimeSpan.TryParse(GetTimezone(), out TimeSpan offset);
            var result = await CreateMatchesService.CreateMatchesByDate(createDeleteMatchesRequest, offset, Configuration);
            return Json(result);
        }

        [HttpPost("/Match/CreateLiveMatches")]
        public async Task<JsonResult> CreateLiveMatches([FromBody] CreateDeleteMatchesModel createDeleteMatchesRequest)
        {
            var result = await CreateMatchesService.CreateLiveMatches(createDeleteMatchesRequest, Configuration);
            return Json(result);
        }

        [HttpPost("/Match/CreateAllSiteMatches")]
        public async Task<JsonResult> CreateAllSiteMatches([FromBody] List<CreateDeleteMatchesModel> createDeleteMatchesModels)
        {
            var matchRepository = new MatchRepository(Configuration);
            await SimulatorService.SendCleanExistData(Configuration);

            TimeSpan.TryParse(GetTimezone(), out TimeSpan offset);
            await matchRepository.CreateAllMatchesSite(createDeleteMatchesModels, offset);

            return Json(true);
        }

        [HttpPost("/Match/DeleteMatchesByDate")]
        public async Task<JsonResult> DeleteMatchesByDate([FromBody] CreateDeleteMatchesModel createDeleteMatchesRequest)
        {
            TimeSpan.TryParse(GetTimezone(), out TimeSpan offset);
            var matchRepository = new MatchRepository();
            var fromDate = new DateTime(createDeleteMatchesRequest.EventDate.Year, createDeleteMatchesRequest.EventDate.Month, createDeleteMatchesRequest.EventDate.Day, 0, 0, 0, DateTimeKind.Utc).Subtract(offset);
            var toDate = new DateTime(createDeleteMatchesRequest.EventDate.Year, createDeleteMatchesRequest.EventDate.Month, createDeleteMatchesRequest.EventDate.Day, 23, 59, 59, DateTimeKind.Utc).Subtract(offset);
            await matchRepository.DeleteMatchByDate(fromDate, toDate, createDeleteMatchesRequest.Count);

            return Json(true);
        }

        [HttpPost("/Match/DeleteLiveMatches")]
        public async Task<JsonResult> DeleteLiveMatches([FromBody] CreateDeleteMatchesModel createDeleteMatchesRequest)
        {
            var matchRepository = new MatchRepository();
            await matchRepository.DeleteLiveMatches(createDeleteMatchesRequest.Count);

            return Json(true);
        }

        [HttpPost("Match/DeleteAllMatches")]
        public async Task<JsonResult> DeleteAllMatches()
        {
            var matchRepository = new MatchRepository();
            await matchRepository.DeleteAllMatches();

            return Json(true);
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

        public async Task<ActionResult> Delete(string id, DateTime searchDate, string leagueId)
        {
            var repository = new MatchRepository();
            await repository.DeleteMatchById(id);
            return await ProcessToDateSearchView(searchDate, leagueId);
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
                TimeSpan offset;
                TimeSpan.TryParse(GetTimezone(), out offset);
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

                requestBody.payload.sport_event_status.aggregate_away_score = int.Parse(collection["aggregate_away_score"]);
                requestBody.payload.sport_event_status.aggregate_home_score = int.Parse(collection["aggregate_home_score"]);
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
                    return View();
                }
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(Configuration.GetValue<string>("Simulator:Url"));
                    var response = await client.PostAsJsonAsync(Configuration.GetValue<string>("Simulator:PushEvent"), requestBody);
                    if (!response.IsSuccessStatusCode)
                    {
                        return RedirectToAction(nameof(Details), new { id });
                    }
                }
                await matchRepository.InsertTimelineLog(requestBody);
                await Task.Delay(2000);
                return RedirectToAction(nameof(Details), new { id });
            }
            catch (Exception ex)
            {
                return RedirectToAction(nameof(Details), new { id });
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
                TimeSpan offset;
                TimeSpan.TryParse(GetTimezone(), out offset);
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
                TimeSpan offset;
                TimeSpan.TryParse(GetTimezone(), out offset);
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
                return RedirectToAction(nameof(Details), new { id });
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
                TimeSpan offset;
                TimeSpan.TryParse(GetTimezone(), out offset);
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
                TimeSpan offset;
                TimeSpan.TryParse(GetTimezone(), out offset);
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SetUpTimezone(IFormCollection collection)
        {
            HttpContext.Session.SetString("Timezone", collection["Timezone"]);
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> CreateScenario(string matchId, IFormCollection collection)
        {
            var matchRepo = new MatchRepository();
            var scenarioRepo = new ScenarioRepository();
            var match = await matchRepo.GetMatchById(matchId);
            var timelines = await matchRepo.GetMatchTimelines(matchId);

            var id = UniqueGenerator.GetIntId();
            var homeId = match.Teams.FirstOrDefault(x => x.IsHome).Id;
            var awayId = match.Teams.FirstOrDefault(x => !x.IsHome).Id;
            var criteria = new CreateScenarioCriteria() { Id = id, Name = collection["scenarioName"], AwayId = awayId, HomeId = homeId, EventDate = match.EventDate };
            await scenarioRepo.CreateNewScenario(criteria);
            for (int i = 0; i < timelines.Count(); i++)
            {
                var timeline = timelines.ElementAt(i);
                await scenarioRepo.AddEventToScenario(id, i + 1, timeline.RequestLog);
            }
            return RedirectToAction(nameof(Details), new { id = matchId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ApplyScenario(string matchId, IFormCollection collection)
        {
            var matchRepo = new MatchRepository();
            var scenarioRepo = new ScenarioRepository();
            var match = await matchRepo.GetMatchById(matchId);
            var homeId = match.Teams.FirstOrDefault(x => x.IsHome).Id;
            var awayId = match.Teams.FirstOrDefault(x => !x.IsHome).Id;
            var scenario = await scenarioRepo.GetScenario(int.Parse(collection["scenarioId"]));
            var differTime = match.EventDate - scenario.EventDate;
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(Configuration.GetValue<string>("Simulator:Url"));
                foreach (var scenarioEvent in scenario.ScenarioEvents)
                {
                    MatchPushEventDto preEvent = JsonConvert.DeserializeObject<MatchPushEventDto>(scenarioEvent.EventRequest);
                    preEvent.metadata.sport_event_id = match.Id;
                    preEvent.metadata.tournament_id = match.League.Id;
                    preEvent.payload.timeline.time = preEvent.payload.timeline.time + differTime;
                    if (preEvent.payload.sport_event_status.aggregate_winner_id == scenario.HomeId) preEvent.payload.sport_event_status.aggregate_winner_id = homeId;
                    if (preEvent.payload.sport_event_status.winner_id == scenario.HomeId) preEvent.payload.sport_event_status.winner_id = homeId;
                    if (preEvent.payload.sport_event_status.aggregate_winner_id == scenario.AwayId) preEvent.payload.sport_event_status.aggregate_winner_id = awayId;
                    if (preEvent.payload.sport_event_status.winner_id == scenario.AwayId) preEvent.payload.sport_event_status.winner_id = awayId;

                    await client.PostAsJsonAsync(Configuration.GetValue<string>("Simulator:PushEvent"), preEvent);

                    await matchRepo.InsertTimelineLog(preEvent);
                }
            }

            return RedirectToAction(nameof(Details), new { id = matchId });
        }

        private string GetTimezone()
        {
            var result = HttpContext.Session.GetString("Timezone");
            if (string.IsNullOrEmpty(result))
            {
                result = "07:00:00";
            }
            return result;
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Score247.Management.Site.Criterias.Match;
using Score247.Management.Site.Helpers;
using Score247.Management.Site.Models;
using Score247.Management.Site.Models.Events;
using Score247.Management.Site.Repositories.Leagues;
using Score247.Management.Site.Repositories.Matches;
using Score247.Management.Site.Repositories.Matches.Models;
using Score247.Management.Site.Repositories.Teams;
using Score247.Management.Site.Services.Matchs;
using Score247.Management.Site.ViewModels.Match;
using Score247Management.Core._Shared.Enumerations;

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
            var fromDate = new DateTime(timeNow.Year, timeNow.Month, timeNow.Day, 0, 0, 0, DateTimeKind.Utc);
            var toDate = new DateTime(timeNow.Year, timeNow.Month, timeNow.Day, 23, 59, 59, DateTimeKind.Utc);
            viewModal.DateSearchModel = new DateSearchModel()
            {
                FromDate = fromDate,
                ToDate = toDate,
                Leagues = leagues,
                SelectedLeagueId = "all"
            };
            var matches = await repository.GetMatchByDataRange(viewModal.DateSearchModel);
            viewModal.Matches = matches;
            return View(viewModal);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> _DateRangeSearch(IFormCollection collection)
        {
            try
            {
                var fromDate = DateTime.Parse(collection["FromDate"]);
                fromDate = new DateTime(fromDate.Year, fromDate.Month, fromDate.Day, 0, 0, 0, DateTimeKind.Utc);
                var toDate = DateTime.Parse(collection["ToDate"]);
                toDate = new DateTime(toDate.Year, toDate.Month, toDate.Day, 23, 59, 59, DateTimeKind.Utc);
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
            catch (Exception ex)
            {
                throw;
            }
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
                // TODO: Add insert logic here
                var leagueRepo = new LeagueRepository();
                var leagues = await leagueRepo.GetLeagues();

                var teamRepo = new TeamRepository();
                var teams = await teamRepo.GetTeams();

                var MatchEntity = new MatchEntity();
                MatchEntity.Id = UniqueGenerator.GetIdWithPattern("sr:match:");
                MatchEntity.Status = collection["MatchEntity.Status"];
                MatchEntity.Venue = new Score247Management.Core.Matches.Models.Venue
                {
                    Id = "sr:venue:" + collection["MatchEntity.Venue.Name"],
                    Name = collection["MatchEntity.Venue.Name"],
                    CountryName = collection["MatchEntity.Venue.CountryName"]
                };
                MatchEntity.Tournament = leagues.FirstOrDefault(x => x.Id == collection["MatchEntity.Tournament.Id"]);
                var competitors = new List<Score247Management.Core.Teams.Models.Team>();
                var homeTeam = teams.FirstOrDefault(x => x.Id == collection["HomeTeam.Id"]);
                homeTeam.Statistic = new Score247Management.Core.Teams.Models.TeamStatistics();
                homeTeam.qualifier = "home";
                var awayTeam = teams.FirstOrDefault(x => x.Id == collection["AwayTeam.Id"]);
                awayTeam.Statistic = new Score247Management.Core.Teams.Models.TeamStatistics();
                awayTeam.qualifier = "away";
                competitors.Add(homeTeam);
                competitors.Add(awayTeam);
                MatchEntity.Competitors = competitors;
                var dateTime = Convert.ToDateTime(collection["MatchEntity.Scheduled"]);
                var timeSpan = TimeSpan.Parse(collection["MatchEntity.ScheduledTime"]);
                MatchEntity.Scheduled = new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, timeSpan.Hours, timeSpan.Minutes, timeSpan.Seconds, DateTimeKind.Utc);
                MatchEntity.TournamentRound = new Score247Management.Core.Leagues.Models.LeagueRound();
                MatchEntity.TournamentRound.Type = LeagueRoundType.FromDisplayName<LeagueRoundType>(collection["MatchEntity.TournamentRound.Type.Value"]);
                MatchEntity.TournamentRound.Name = collection["MatchEntity.TournamentRound.Name"];
                MatchEntity.TournamentRound.Number = Int32.Parse(collection["MatchEntity.TournamentRound.Number"]);
                MatchEntity.TournamentRound.Phase = collection["MatchEntity.TournamentRound.Phase"];
                MatchEntity.TournamentRound.Group = collection["MatchEntity.TournamentRound.Group"];
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(Configuration.GetValue<string>("Simulator:Url"));
                    var MatchEntityes = new List<MatchEntity> { MatchEntity };
                    var content = new Repositories.Matches.Models.DailyScheduleDto
                    {
                        generated_at = DateTime.UtcNow,
                        schema = "Matches",
                        sport_events = MatchEntityes
                    };
                    var jsonValue = JsonConvert.SerializeObject(content);

                    var beginDateToday = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, DateTime.UtcNow.Day, 0, 0, 0, DateTimeKind.Utc);
                    var requestPath = Configuration.GetValue<string>("Simulator:DailySchedule");

                    var response = await client.PostAsync(string.Format(requestPath + "?date={0}", MatchEntity.Scheduled.ToString("yyyy-MM-dd")), new StringContent(jsonValue, Encoding.UTF8, "application/json"));
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
                    await triggerClient.SendAsync(request);
                    var matchRepository = new MatchRepository();
                    var matchCreated = await matchRepository.GetMatchById(MatchEntity.Id);
                    while (matchCreated == null)
                    {
                        await Task.Delay(1000);
                        matchCreated = await matchRepository.GetMatchById(MatchEntity.Id);
                    }
                }
                return RedirectToAction(nameof(Details), new { id = MatchEntity.Id });
            }
            catch (Exception ex)
            {
                return View();
            }
        }

        // GET: Match/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> CloneMatches(IFormCollection collection)
        {
            try
            {
                var criteria = new MatchCloneCriteria();
                var eventDate = DateTime.Parse(collection["CloneMatchModel.EventDate"]);
                criteria.EventDate = new DateTime(eventDate.Year, eventDate.Month, eventDate.Day, 0, 0, 0, DateTimeKind.Utc);
                //criteria.EventDate = new DateTime(eventDate.Year, eventDate.Month, eventDate.Day, 0, 0, 0, DateTimeKind.Utc);
                criteria.LimitMatch = int.Parse(collection["CloneMatchModel.Count"]);
                var matchRepository = new MatchRepository(Configuration);
                await matchRepository.CreateMatches(criteria);
            }
            catch (Exception ex)
            {
                return RedirectToAction(nameof(Index));
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> CloneLiveMatches(IFormCollection collection)
        {
            try
            {
                var criteria = new GetCloneLiveMatchesCriteria();
                var eventDate = DateTime.Parse(collection["CloneMatchModel.EventDate"]);
                criteria.EventDate = new DateTime(eventDate.Year, eventDate.Month, eventDate.Day, 0, 0, 0, DateTimeKind.Utc);
                criteria.LimitMatch = int.Parse(collection["CloneMatchModel.Count"]);
                var matchRepository = new MatchRepository();
                await matchRepository.CloneLiveMatches(criteria, Configuration.GetValue<string>("Simulator:Url"), Configuration.GetValue<string>("Simulator:PushEvent"));
            }
            catch (Exception ex)
            {
                return RedirectToAction(nameof(Index));
            }
            return RedirectToAction(nameof(Index));
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
        public async Task<ActionResult> DeleteMatchesByDate(IFormCollection collection)
        {
            try
            {
                var eventDate = DateTime.Parse(collection["DeleteMatchByDateCriteria.DeleteDate"]);
                var matchRepository = new MatchRepository();
                await matchRepository.DeleteMatchByDate(eventDate);

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                return RedirectToAction(nameof(Index));
            }
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
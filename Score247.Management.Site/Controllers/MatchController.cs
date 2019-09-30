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

namespace Score247.Management.Site.Controllers
{
    public class MatchController : Controller
    {
        private readonly IMatchService matchService;
        private readonly ICreateMatchesService createMatchesService;
        private readonly IMatchEventsService matchEventsService;
        private readonly IScenarioService scenarioService;
        private readonly IAutoEventService autoEventService;

        public MatchController(
            IMatchService matchService,
            ICreateMatchesService createMatchesService,
            IMatchEventsService matchEventsService,
            IScenarioService scenarioService,
            IAutoEventService autoEventService)
        {
            this.createMatchesService = createMatchesService;
            this.matchService = matchService;
            this.matchEventsService = matchEventsService;
            this.scenarioService = scenarioService;
            this.autoEventService = autoEventService;
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

            TempData["SearchDate"] = timeNow;
            TempData["LeagueId"] = "all";
            return View(viewModal);
        }

        private List<QuickDateSelectOption> GenerateQuickDateSelectOptions(TimeSpan offset)
        {
            var options = new List<QuickDateSelectOption>();
            var today = DateTime.UtcNow;
            options.Add(new QuickDateSelectOption(today.AddDays(-2).Add(offset)));
            options.Add(new QuickDateSelectOption(today.AddDays(-1).Add(offset)));
            options.Add(new QuickDateSelectOption() { DisplayName = "TODAY", DisplayDate = today.Add(offset).ToString("MMM dd").ToUpper(), Value = today.Add(offset).ToString("MM/dd/yyyy") });
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

            var matches = await matchService.SearchMatchByDateAndLeague(searchDate, selectedLeagueId, offset);
            viewModal.Matches = matches;
            viewModal.CreateDeleteMatchesModels = CreateDeleteMatchesModel.CreateFromToDay();
            viewModal.QuickDateSelectOptions = GenerateQuickDateSelectOptions(offset);

            TempData["SearchDate"] = searchDate;
            TempData["LeagueId"] = selectedLeagueId;
            return View("Index", viewModal);
        }

        // GET: Match/Details/5
        public async Task<ActionResult> Details(string id)
        {
            var ran = new Random();
            var repository = new MatchRepository();
            var scenarioRepo = new ScenarioRepository();

            TimeSpan.TryParse(GetTimezone(), out TimeSpan offset);
            var match = await repository.GetMatchById(id, offset);
            var timelines = await repository.GetMatchTimelines(id, offset);
            var viewModel = new EventMatchViewModel
            {
                Match = match,
                TimelineEvents = timelines,
                MatchCardEventDto = new MatchPushEventDto(match, offset)
            };
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

        // GET: Match/Create
        public async Task<ActionResult> Create()
        {
            var leagueRepo = new LeagueRepository();
            var leagues = await leagueRepo.GetLeagues();

            var teamRepo = new TeamRepository();
            var teams = await teamRepo.GetTeams();

            var viewModel = new CreateMatchViewModel
            {
                Tournaments = leagues,
                Teams = teams,
                Match = new MatchEntity()
            };
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
                var matchId = await createMatchesService.CreateMatchWithForm(collection, offset);
                if (string.IsNullOrEmpty(matchId))
                {
                    return View();
                }
                return RedirectToAction(nameof(Details), new { id = matchId, timeSpan = collection["Timezone"].ToString() });
            }
            catch (Exception)
            {
            }

            return View();
        }

        [HttpGet("/Match/AutoCreateMatch")]
        public async Task<JsonResult> AutoCreateMatch(DateTimeOffset createDate)
            => Json(new
            {
                matchId = await createMatchesService.AutoCreateMatch(createDate)
            });

        [HttpPost("/Match/CreateMatchesByDate")]
        public async Task<JsonResult> CreateMatchesByDate([FromBody] CreateDeleteMatchesModel createDeleteMatchesRequest)
        {
            TimeSpan.TryParse(GetTimezone(), out TimeSpan offset);
            var result = await createMatchesService.CreateMatchesByDate(createDeleteMatchesRequest, offset);
            return Json(result);
        }

        [HttpPost("/Match/CreateLiveMatches")]
        public async Task<JsonResult> CreateLiveMatches([FromBody] CreateLiveMatchModel createLiveMatchModel)
        {
            await matchService.CreateLiveMatches(createLiveMatchModel);

            return Json(true);
        }

        [HttpPost("/Match/CreateAllSiteMatches")]
        public async Task<JsonResult> CreateAllSiteMatches([FromBody] List<CreateDeleteMatchesModel> createDeleteMatchesModels)
        {
            TimeSpan.TryParse(GetTimezone(), out TimeSpan offset);
            await createMatchesService.CreateAllMatchesSite(createDeleteMatchesModels, offset);

            return Json(true);
        }

        [HttpPost("/Match/DeleteMatchesByDate")]
        public async Task<JsonResult> DeleteMatchesByDate([FromBody] CreateDeleteMatchesModel createDeleteMatchesRequest)
        {
            TimeSpan.TryParse(GetTimezone(), out TimeSpan offset);
            await matchService.DeleteMatchesByDate(createDeleteMatchesRequest, offset);

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

        [HttpGet("Match/GetMatchesByDate")]
        public async Task<ActionResult> GetMatchesByDate(DateTime searchDate, string leagueId)
        {
            TimeSpan.TryParse(GetTimezone(), out TimeSpan offset);
            var matches = await matchService.SearchMatchByDateAndLeague(searchDate, leagueId, offset);

            TempData["SearchDate"] = searchDate;
            TempData["LeagueId"] = leagueId;

            return PartialView("NormalMatchListPartial", matches);
        }

        [HttpGet("Match/GetLiveMatches")]
        public async Task<ActionResult> GetLiveMatches()
        {
            var matches = await matchService.GetLiveMatch("en-US");

            return PartialView("LiveMatchListPartial", matches);
        }

        [HttpPost("Match/CloseLiveMatch")]
        public async Task<ActionResult> CloseLiveMatch(string matchId)
        {
            await matchService.CloseLiveMatch(matchId);

            return Json(true);
        }

        public async Task<ActionResult> CreateActiveLeagueMatch(IFormCollection collection)
        {
            TimeSpan.TryParse(GetTimezone(), out TimeSpan offset);
            await matchService.CreateMatchForActiveLeagues(DateTime.Now, offset);

            return RedirectToAction(nameof(Index));
        }

        public async Task<ActionResult> Delete(string id, DateTime searchDate, string leagueId)
        {
            var repository = new MatchRepository();
            await repository.DeleteMatchById(id);
            return await ProcessToDateSearchView(searchDate, leagueId);
        }

        [HttpGet("/Match/AutoCreateEvent")]
        public async Task<ActionResult> AutoCreateMatchEvent(string matchId, string eventName, DateTimeOffset time)
            => await _CreateTimeEventPartial(matchId, new FormCollection(new Dictionary<string, StringValues>
            {
                { "payload.timeline.type", new StringValues(eventName) },
                { "payload.timeline.time", new StringValues(time.Date.ToString()) },
                { "payload.timeline.hour", new StringValues(new TimeSpan(time.Hour, time.Minute, time.Second).ToString()) }
            }), true);

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> _CreateTimeEventPartial(string id, IFormCollection collection, bool isFromAjax = false)
        {
            try
            {
                TimeSpan.TryParse(GetTimezone(), out TimeSpan offset);
                await matchEventsService.CreateTimeEventPartial(id, collection, offset);

                if (isFromAjax)
                {
                    return Json(new { message = "OK" });
                }
            }
            catch (Exception)
            {
            }

            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> _CreateScoreEventPartial(string id, IFormCollection collection)
        {
            try
            {
                TimeSpan.TryParse(GetTimezone(), out TimeSpan offset);
                await matchEventsService.CreateScoreEventPartial(id, collection, offset);
            }
            catch (Exception)
            {
            }

            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> _CreatePenaltyShootEventPartial(string id, IFormCollection collection)
        {
            try
            {
                TimeSpan.TryParse(GetTimezone(), out TimeSpan offset);
                await matchEventsService.CreatePenaltyShootEventPartial(id, collection, offset);
            }
            catch (Exception)
            {
            }

            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> CreatCardFaultEventPartial(string id, IFormCollection collection)
        {
            try
            {
                TimeSpan.TryParse(GetTimezone(), out TimeSpan offset);

                await matchEventsService.CreatCardFaultEventPartial(id, collection, offset);
            }
            catch (Exception)
            {
            }

            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> CreateInjuryExtraTimeEventPartial(string id, IFormCollection collection)
        {
            try
            {
                TimeSpan.TryParse(GetTimezone(), out TimeSpan offset);
                await matchEventsService.CreateInjuryExtraTimeEventPartial(id, collection, offset);
            }
            catch (Exception)
            {
            }

            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AutoEventNormalMatch(string id, IFormCollection collection)
        {
            try
            {
                autoEventService.PushNormalMatchEvents(id);
            }
            catch (Exception)
            {
            }

            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AutoEventOvertimeMatch(string id, IFormCollection collection)
        {
            try
            {
                autoEventService.PushOvertimeMatchEvents(id);
            }
            catch (Exception)
            {
            }

            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AutoEventPenaltyMatch(string id, IFormCollection collection)
        {
            try
            {
                autoEventService.PushPenaltyMatchEvents(id);
            }
            catch (Exception)
            {
            }

            return RedirectToAction(nameof(Details), new { id });
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
            await scenarioService.CreateScenario(matchId, collection);
            
            return RedirectToAction(nameof(Details), new { id = matchId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ApplyScenario(string matchId, IFormCollection collection)
        {
            await scenarioService.ApplyScenario(matchId, collection);

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
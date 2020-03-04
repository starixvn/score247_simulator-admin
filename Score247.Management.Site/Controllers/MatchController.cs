using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Fanex.Logging;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Score247.Management.Site.Models;
using Score247.Management.Site.Models.Events;
using Score247.Management.Site.Repositories.Leagues;
using Score247.Management.Site.Repositories.Matches;
using Score247.Management.Site.Repositories.Scenarios;
using Score247.Management.Site.Repositories.Teams;
using Score247.Management.Site.Services;
using Score247.Management.Site.Services.Matches;
using Score247.Management.Site.ViewModels.Matches;

namespace Score247.Management.Site.Controllers
{
    [Authorize]
    public class MatchController : Controller
    {
        private const string SearchDate = "SearchDate";
        private const string HourFormat = @"hh\:mm";
        private const string Timezone = "Timezone";
        private const int DelayMilisecoundForJobRun = 2000;
        private readonly IMatchService matchService;
        private readonly ICreateMatchesService createMatchesService;
        private readonly IMatchEventsService matchEventsService;
        private readonly IScenarioService scenarioService;
        private readonly IAutoEventService autoEventService;
        private readonly IQueryService queryService;
        private readonly ISimulatorService simulatorService;

        public MatchController(
            IMatchService matchService,
            ICreateMatchesService createMatchesService,
            IMatchEventsService matchEventsService,
            IScenarioService scenarioService,
            IAutoEventService autoEventService,
            IQueryService queryService,
            ISimulatorService simulatorService)
        {
            this.createMatchesService = createMatchesService;
            this.matchService = matchService;
            this.matchEventsService = matchEventsService;
            this.scenarioService = scenarioService;
            this.autoEventService = autoEventService;
            this.queryService = queryService;
            this.simulatorService = simulatorService;
        }

        // GET: Match
        public async Task<ActionResult> Index()
        {
            var leagueRepository = new LeagueRepository();
            var leagues = await leagueRepository.GetLeagues();
            TimeSpan.TryParse(GetTimezone(), out TimeSpan offset);
            var viewModal = new MatchViewModel();
            var timeNow = DateTime.UtcNow.Add(offset);
            viewModal.DateSearchModel = new DateSearchModel
            {
                SearchDate = timeNow,
                Leagues = leagues,
                SelectedLeagueId = "all"
            };
            var matches = await matchService.SearchMatchByDateAndLeague(timeNow, "all", offset);
            viewModal.Matches = matches;
            viewModal.CreateDeleteMatchesModels = CreateDeleteMatchesModel.CreateFromToDay();
            viewModal.QuickDateSelectOptions = GenerateQuickDateSelectOptions(offset);

            TempData[SearchDate] = timeNow;
            TempData["LeagueId"] = "all";
            TempData[Timezone] = offset.ToString(HourFormat);
            return View(viewModal);
        }

        private List<QuickDateSelectOption> GenerateQuickDateSelectOptions(TimeSpan offset)
        {
            var options = new List<QuickDateSelectOption>();
            var today = DateTime.UtcNow;
            options.Add(new QuickDateSelectOption(today.AddDays(-2).Add(offset)));
            options.Add(new QuickDateSelectOption(today.AddDays(-1).Add(offset)));
            options.Add(new QuickDateSelectOption { DisplayName = "TODAY", DisplayDate = today.Add(offset).ToString("MMM dd").ToUpperInvariant(), Value = today.Add(offset).ToString("MM/dd/yyyy") });
            options.Add(new QuickDateSelectOption(today.AddDays(1).Add(offset)));
            options.Add(new QuickDateSelectOption(today.AddDays(2).Add(offset)));

            return options;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> _DateRangeSearch(IFormCollection collection)
        {
            var eventDate = DateTime.Parse(collection[SearchDate]);
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
                DateSearchModel = new DateSearchModel
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

            TempData[SearchDate] = searchDate;
            TempData["LeagueId"] = selectedLeagueId;
            TempData[Timezone] = offset.ToString(HourFormat);
            return View("Index", viewModal);
        }

        // GET: Match/Details/5
        public async Task<ActionResult> Details(string id)
        {
            var repository = new MatchRepository();
            var scenarioRepo = new ScenarioRepository();

            TimeSpan.TryParse(GetTimezone(), out TimeSpan offset);
            var match = await repository.GetMatchById(id, offset);
            var timelines = await repository.GetMatchTimelines(id, offset);
            var viewModel = new EventMatchViewModel(match, timelines, offset)
            {
                Scenarios = await scenarioRepo.GetScenarios(),
                Statistic = await queryService.GetMatchStatistic(id, "en-US"),
                MatchPitchViewLineups = await queryService.GetLineUps(id, "en-US"),
                LineUpsDto = await simulatorService.GetLineUpsDto(id)
            };

            TempData[Timezone] = offset.ToString(HourFormat);

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
                return RedirectToAction(nameof(Details), new { id = matchId, timeSpan = collection[Timezone].ToString() });
            }
            catch (Exception ex)
            {
                return View();
            }
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
            await Task.Delay(DelayMilisecoundForJobRun);

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
            try
            {
                await matchService.CloseAllLiveMatches();
                await matchService.DeleteAllMatches();

                return Json(true);
            }
            catch (Exception ex)
            {
                Logger.Log.Error(ex.Message);
            }

            return Json(false);
        }

        [HttpGet("Match/GetMatchesByDate")]
        public async Task<ActionResult> GetMatchesByDate(DateTime searchDate, string leagueId)
        {
            TimeSpan.TryParse(GetTimezone(), out TimeSpan offset);
            var matches = await matchService.SearchMatchByDateAndLeague(searchDate, leagueId, offset);

            TempData[SearchDate] = searchDate;
            TempData["LeagueId"] = leagueId;
            TempData[Timezone] = offset.ToString(HourFormat);

            return PartialView("NormalMatchListPartial", matches);
        }

        [HttpGet("Match/GetLiveMatches")]
        public async Task<ActionResult> GetLiveMatches()
        {
            TimeSpan.TryParse(GetTimezone(), out TimeSpan offset);
            var matches = await matchService.GetLiveMatch("en-US", offset);
            TempData[Timezone] = offset.ToString(HourFormat);

            return PartialView("LiveMatchListPartial", matches);
        }

        [HttpPost("Match/CloseLiveMatch")]
        public async Task<ActionResult> CloseLiveMatch(string matchId)
        {
            await matchService.CloseLiveMatch(matchId);

            return Json(true);
        }

        [HttpPost("Match/CloseAllLiveMatches")]
        public async Task<ActionResult> CloseAllLiveMatches()
        {
            await matchService.CloseAllLiveMatches();
            await Task.Delay(DelayMilisecoundForJobRun);

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

        [HttpPost("Match/CreateTimeEventPartial/{id}")]
        public async Task<ActionResult> CreateTimeEventPartial(string id, IFormCollection collection)
        {
            TimeSpan.TryParse(GetTimezone(), out TimeSpan offset);
            var eventDto = await matchEventsService.CreateTimeEventPartial(id, collection, offset);

            return Json(eventDto);
        }

        [HttpPost("Match/CreateScoreEventPartial/{id}")]
        public async Task<ActionResult> CreateScoreEventPartial(string id, IFormCollection collection)
        {
            TimeSpan.TryParse(GetTimezone(), out TimeSpan offset);
            var eventDto = await matchEventsService.CreateScoreEventPartial(id, collection, offset);

            return Json(eventDto);
        }

        [HttpPost("Match/CreatePenaltyShootEventPartial/{id}")]
        public async Task<ActionResult> CreatePenaltyShootEventPartial(string id, IFormCollection collection)
        {
            TimeSpan.TryParse(GetTimezone(), out TimeSpan offset);
            var eventDto = await matchEventsService.CreatePenaltyShootEventPartial(id, collection, offset);

            return Json(eventDto);
        }

        [HttpPost("Match/CreateCardFaultEventPartial/{id}")]
        public async Task<ActionResult> CreateCardFaultEventPartial(string id, IFormCollection collection)
        {
            TimeSpan.TryParse(GetTimezone(), out TimeSpan offset);
            var eventDto = await matchEventsService.CreateCardFaultEventPartial(id, collection, offset);

            return Json(eventDto);
        }

        [HttpPost("Match/CreateInjuryExtraTimeEventPartial/{id}")]
        public async Task<ActionResult> CreateInjuryExtraTimeEventPartial(string id, IFormCollection collection)
        {
            TimeSpan.TryParse(GetTimezone(), out TimeSpan offset);
            var eventDto = await matchEventsService.CreateInjuryExtraTimeEventPartial(id, collection, offset);

            return Json(eventDto);
        }

        [HttpPost("Match/CreateSubstitutionEventPartial/{id}")]
        public async Task<ActionResult> CreateSubstitutionEventPartial(string id, IFormCollection collection)
        {
            TimeSpan.TryParse(GetTimezone(), out TimeSpan offset);
            var eventDto = await matchEventsService.CreateSubstitutionEventPartial(id, collection, offset);

            return Json(eventDto);
        }

        [HttpPost("Match/CreateMinorEventPartial/{id}")]
        public async Task<ActionResult> CreateMinorEventPartial(string id, IFormCollection collection)
        {
            TimeSpan.TryParse(GetTimezone(), out TimeSpan offset);
            var eventDto = await matchEventsService.CreateMinorEventPartial(id, collection, offset);

            return Json(eventDto);
        }

        [HttpPost("Match/CreateVAREventPartial/{id}")]
        public async Task<ActionResult> CreateVAREventPartial(string id, IFormCollection collection)
        {
            TimeSpan.TryParse(GetTimezone(), out TimeSpan offset);
            var eventDto = await matchEventsService.CreateVAREventPartial(id, collection, offset);

            return Json(eventDto);
        }

        [HttpPost("/Match/Event/Apply")]
        public async Task<ActionResult> ApplyEventTimeline([FromBody] MatchPushEventDto matchPushEventDto, [FromQuery] bool eventMissed)
        {
            await matchEventsService.SendEventTimeline(matchPushEventDto, eventMissed);

            return Json(true);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AutoEventNormalMatch(string id, IFormCollection collection)
        {
            autoEventService.PushNormalMatchEvents(id);

            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AutoEventOvertimeMatch(string id, IFormCollection collection)
        {
            autoEventService.PushOvertimeMatchEvents(id);

            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AutoEventPenaltyMatch(string id, IFormCollection collection)
        {
            autoEventService.PushPenaltyMatchEvents(id);

            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SetUpTimezone(IFormCollection collection)
        {
            HttpContext.Session.SetString(Timezone, collection[Timezone]);
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
        public ActionResult ApplyScenario(string matchId, IFormCollection collection)
        {
            matchService.ApplyMatchScenarios(matchId, collection);

            return RedirectToAction(nameof(Details), new { id = matchId });
        }

        private string GetTimezone()
        {
            var result = HttpContext.Session.GetString(Timezone);
            if (string.IsNullOrEmpty(result))
            {
                result = "07:00:00";
            }

            return result;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ApplyMatchStatistic(string matchId, IFormCollection collection)
        {
            await matchService.ApplyMatchStatistic(matchId, collection);

            return RedirectToAction(nameof(Details), new { id = matchId });
        }

        public async Task<IActionResult> EnableDisableCoverage(string id)
        {
            await matchService.EnableDisableCoverage(id);

            return RedirectToAction(nameof(Details), new { id });
        }

        // Line-Ups

        [HttpPost("/Match/Lineups")]
        public async Task<ActionResult> GenerateLineups([FromQuery] string matchId, [FromQuery]  string homeFormation, [FromQuery] string awayFormation)
        {
            var lineupsDto = await matchService.GenerateLineups(matchId, homeFormation, awayFormation);

            return Json(lineupsDto);
        }

        [HttpPost("/Match/Lineups/{matchId}")]
        public async Task<ActionResult> ApplyLineups(string matchId, [FromBody] LineUpsDto lineUpsDto)
        {
            await matchService.ApplyLineups(lineUpsDto);

            return Json(true);
        }

        [HttpGet("/Match/Lineups/{matchId}")]
        public async Task<ActionResult> GetLineups(string matchId)
        {
            var lineupsData = await matchService.GetLineups(matchId);

            return Json(lineupsData);
        }

        // H2H

        [HttpPost("/Team/H2H/Generate")]
        public async Task<ActionResult> GenerateH2H([FromQuery]  string matchId, [FromQuery] string h2hCase)
        {
            var h2HDto = await matchService.GenerateH2H(matchId, h2hCase);

            return Json(h2HDto);
        }

        [HttpPost("/Team/H2H/Apply/{matchId}")]
        public async Task<ActionResult> ApplyH2H(string matchId, [FromBody] H2HDto h2HDto)
        {
            await matchService.ApplyH2H(h2HDto);

            return Json(true);
        }

        [HttpGet("/Team/H2H/{matchId}")]
        public async Task<ActionResult> GetH2H(string matchId)
        {
            var h2hData = await matchService.GetH2H(matchId);

            return Json(h2hData);
        }

        [HttpPost("/Match/Timeline/Remove/{matchId}/{timelineId}")]
        public async Task<ActionResult> RemoveTimeline(string matchId, string timelineId)
        {
            await matchService.RemoveTimeline(matchId, timelineId);

            return Json(true);
        }
    }
}
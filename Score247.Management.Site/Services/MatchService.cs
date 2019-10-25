using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Score247.Management.Site.Models;
using Score247.Management.Site.Models.DataMappers;
using Score247.Management.Site.Models.Events;
using Score247.Management.Site.Repositories.Matches;
using Score247.Management.Site.Services.Matches;
using Score247Management.Core.Matches.Models.Soccer.Core.Matches.Models;
using Score247Management.Core.Statistics.Models;
using Score247Management.Core.Teams.Models;
using MatchSummary = Score247Management.Core.Matches.Models.MatchSummary;

namespace Score247.Management.Site.Services
{
    public interface IMatchService
    {
        Task CreateMatchForActiveLeagues(DateTime createDate, TimeSpan offset);

        Task<IEnumerable<MatchSummary>> GetLiveMatch(string language, TimeSpan offset);

        Task CreateLiveMatches(CreateLiveMatchModel createLiveMatchModel);

        Task CloseLiveMatch(string matchId);

        Task DeleteMatchesByDate(CreateDeleteMatchesModel createDeleteMatchesRequest, TimeSpan offset);

        Task<IEnumerable<MatchSummary>> SearchMatchByDateAndLeague(DateTime dateSearch, string leagueId, TimeSpan offset);

        Task CloseAllLiveMatches();

        Task<Result> GetSimulatorResult(string matchId);

        Task SendCustomLiveMatch(string matchId, Sport_Event_Status sportEventStatus);

        Task ApplyMatchStatistic(string matchId, IFormCollection collection);

        Task EnableDisableCoverage(string matchId);

        Task DeleteAllMatches();

        Task ApplyMatchScenarios(string matchId, IFormCollection collection);

        Task<LineUpsDto> GenerateLineups(string matchId, string homeFormation, string awayFormation);

        Task ApplyLineups(LineUpsDto lineUpsDto);

        Task ApplyH2H(H2HDto h2HDto);

        Task<H2HDto> GenerateH2H(string matchId);
    }

    public class MatchService : IMatchService
    {
        private readonly ISimulatorService simulatorService;
        private readonly IQueryService queryService;
        private readonly ITriggerJobService triggerJobService;
        private readonly IScenarioService scenarioService;
        private readonly ITimelineService timelineService;
        private readonly ILineupService lineupService;
        private readonly ICreateMatchesService createMatchesService;
        private readonly IMatchFileService matchFileService;
        private readonly IH2HService h2HService;

        public MatchService(
            ISimulatorService simulatorService,
            IQueryService queryService,
            ITriggerJobService triggerJobService,
            IScenarioService scenarioService,
            ITimelineService timelineService,
            ILineupService lineupService,
            ICreateMatchesService createMatchesService,
            IMatchFileService matchFileService,
            IH2HService h2HService)
        {
            this.simulatorService = simulatorService;
            this.queryService = queryService;
            this.triggerJobService = triggerJobService;
            this.scenarioService = scenarioService;
            this.timelineService = timelineService;
            this.lineupService = lineupService;
            this.createMatchesService = createMatchesService;
            this.matchFileService = matchFileService;
            this.h2HService = h2HService;
        }

        public async Task CreateMatchForActiveLeagues(DateTime createDate, TimeSpan offset)
        {
            await createMatchesService.CreateMatchForActiveLeagues(createDate, offset);
        }

        public async Task<IEnumerable<MatchSummary>> GetLiveMatch(string language, TimeSpan offset)
        {
            var matches = await queryService.GetLiveMatches(language);
            var result = matches.Select(match =>
            {
                match.DisplayDate = match.EventDate.Add(offset).ToString("dd/MM/yyyy HH:mm:ss");
                return match;
            });

            return result;
        }

        public async Task CreateLiveMatches(CreateLiveMatchModel createLiveMatchModel)
        {
            await createMatchesService.CreateLiveMatches(createLiveMatchModel);
        }

        public async Task CloseLiveMatch(string matchId)
        {
            var content = await matchFileService.ReadLiveMatchFromFile();
            if (content != null)
            {
                var newLiveMatches = content.results.Where(x => x.sport_event.Id != matchId);
                content.results = newLiveMatches;
                content.generated_at = DateTime.UtcNow;
            }
            else
            {
                content = new DailyResultDto()
                {
                    generated_at = DateTime.UtcNow,
                    results = new List<Result>(),
                    schema = "Matches"
                };
            }

            matchFileService.SaveLiveMatchToFile(content);
            await simulatorService.SendLiveResult(content);
            await triggerJobService.TriggerFetchLiveMatch();
        }

        public async Task DeleteMatchesByDate(CreateDeleteMatchesModel createDeleteMatchesRequest, TimeSpan offset)
        {
            var matchRepository = new MatchRepository();
            var fromDate = new DateTime(createDeleteMatchesRequest.EventDate.Year, createDeleteMatchesRequest.EventDate.Month, createDeleteMatchesRequest.EventDate.Day, 0, 0, 0, DateTimeKind.Utc).Subtract(offset);
            var toDate = new DateTime(createDeleteMatchesRequest.EventDate.Year, createDeleteMatchesRequest.EventDate.Month, createDeleteMatchesRequest.EventDate.Day, 23, 59, 59, DateTimeKind.Utc).Subtract(offset);
            await matchRepository.DeleteMatchByDate(fromDate, toDate, createDeleteMatchesRequest.Count);
        }

        public async Task<IEnumerable<MatchSummary>> SearchMatchByDateAndLeague(DateTime dateSearch, string leagueId, TimeSpan offset)
        {
            var fromDate = new DateTimeOffset(dateSearch.Year, dateSearch.Month, dateSearch.Day, 0, 0, 0, offset);
            var toDate = new DateTimeOffset(dateSearch.Year, dateSearch.Month, dateSearch.Day, 23, 59, 59, offset);

            var matches = await queryService.GetMatchesByDateRange(fromDate, toDate, "en-US", offset);

            if (leagueId == "all")
            {
                return matches;
            }

            return matches.Where(match => match.LeagueId == leagueId);
        }

        public async Task CloseAllLiveMatches()
        {
            var content = new DailyResultDto()
            {
                generated_at = DateTime.UtcNow,
                schema = "Matches",
                results = new List<Result>()
            };

            matchFileService.SaveLiveMatchToFile(content);

            await simulatorService.SendLiveResult(content);
            await triggerJobService.TriggerFetchLiveMatch();
        }

        public async Task<Result> GetSimulatorResult(string matchId)
        {
            var dailyResult = await matchFileService.ReadLiveMatchFromFile();

            return dailyResult?.results?.FirstOrDefault(result => result.sport_event.Id == matchId);
        }

        public async Task SendCustomLiveMatch(string matchId, Sport_Event_Status sportEventStatus)
        {
            var result = await GetSimulatorResult(matchId);
            if (result == null)
            {
                return;
            }
            sportEventStatus.period_scores = result.sport_event_status.period_scores;

            result.sport_event_status = sportEventStatus;
            result.sport_event.Status = sportEventStatus.status;

            var content = await matchFileService.ReadLiveMatchFromFile() ?? new DailyResultDto
            {
                generated_at = DateTime.UtcNow,
                schema = "Matches",
                results = new List<Result>()
            };
            var newLiveMatches = content.results.Where(x => x.sport_event.Id != matchId).ToList();

            newLiveMatches.Add(result);
            content.results = newLiveMatches;

            matchFileService.SaveLiveMatchToFile(content);
            await simulatorService.SendLiveResult(content);
            await triggerJobService.TriggerFetchLiveMatch();
            await triggerJobService.TriggerFetchLiveMatchTimeline();
        }

        public async Task ApplyMatchStatistic(string matchId, IFormCollection collection)
        {
            var statistic = new Statistic()
            {
                MatchId = matchId,
                HomeStatistic = new TeamStatistic()
                {
                    RedCards = int.Parse(collection["HomeStatistic.RedCards"]),
                    Possession = int.Parse(collection["HomeStatistic.Possession"]),
                    CornerKicks = int.Parse(collection["HomeStatistic.CornerKicks"]),
                    Fouls = int.Parse(collection["HomeStatistic.Fouls"]),
                    FreeKicks = int.Parse(collection["HomeStatistic.FreeKicks"]),
                    GoalKicks = int.Parse(collection["HomeStatistic.GoalKicks"]),
                    Injuries = int.Parse(collection["HomeStatistic.Injuries"]),
                    Offsides = int.Parse(collection["HomeStatistic.Offsides"]),
                    ShotsBlocked = int.Parse(collection["HomeStatistic.ShotsBlocked"]),
                    ShotsOffTarget = int.Parse(collection["HomeStatistic.ShotsOffTarget"]),
                    ShotsOnTarget = int.Parse(collection["HomeStatistic.ShotsOnTarget"]),
                    ShotsSaved = int.Parse(collection["HomeStatistic.ShotsSaved"]),
                    ThrowIns = int.Parse(collection["HomeStatistic.ThrowIns"]),
                    YellowCards = int.Parse(collection["HomeStatistic.YellowCards"]),
                    YellowRedCards = int.Parse(collection["HomeStatistic.YellowRedCards"])
                },
                AwayStatistic = new TeamStatistic()
                {
                    RedCards = int.Parse(collection["AwayStatistic.RedCards"]),
                    Possession = int.Parse(collection["AwayStatistic.Possession"]),
                    CornerKicks = int.Parse(collection["AwayStatistic.CornerKicks"]),
                    Fouls = int.Parse(collection["AwayStatistic.Fouls"]),
                    FreeKicks = int.Parse(collection["AwayStatistic.FreeKicks"]),
                    GoalKicks = int.Parse(collection["AwayStatistic.GoalKicks"]),
                    Injuries = int.Parse(collection["AwayStatistic.Injuries"]),
                    Offsides = int.Parse(collection["AwayStatistic.Offsides"]),
                    ShotsBlocked = int.Parse(collection["AwayStatistic.ShotsBlocked"]),
                    ShotsOffTarget = int.Parse(collection["AwayStatistic.ShotsOffTarget"]),
                    ShotsOnTarget = int.Parse(collection["AwayStatistic.ShotsOnTarget"]),
                    ShotsSaved = int.Parse(collection["AwayStatistic.ShotsSaved"]),
                    ThrowIns = int.Parse(collection["AwayStatistic.ThrowIns"]),
                    YellowCards = int.Parse(collection["AwayStatistic.YellowCards"]),
                    YellowRedCards = int.Parse(collection["AwayStatistic.YellowRedCards"])
                }
            };
            await timelineService.ApplyStatistic(statistic);
            await createMatchesService.UpdateLiveMatch(statistic.MatchId);

            await Task.Delay(2000);
        }

        public async Task EnableDisableCoverage(string matchId)
        {
            var matchRepo = new MatchRepository();
            var match = await matchRepo.GetMatchById(matchId);

            if (match.Coverage != null && match.Coverage.Live)
            {
                match.Coverage = new Coverage(false, "");
            }
            else
            {
                match.Coverage = new Coverage(true, "", true, true, true, true, true);
            }

            await timelineService.UpdatePreMatchInfo(match);
        }

        public async Task DeleteAllMatches()
        {
            await simulatorService.SendCleanExistData();
            await simulatorService.SendCleanTimelines();

            var matchRepository = new MatchRepository();
            await matchRepository.DeleteAllMatches();
        }

        public async Task ApplyMatchScenarios(string matchId, IFormCollection collection)
        {
            await scenarioService.ApplyScenario(matchId, collection);
        }

        public async Task<LineUpsDto> GenerateLineups(string matchId, string homeFormation, string awayFormation)
        {
            var matchRepository = new MatchRepository();
            var match = await matchRepository.GetMatchById(matchId);

            var lineUpDto = lineupService.BuildLineups(match, homeFormation, awayFormation);

            return lineUpDto;
        }

        public async Task ApplyLineups(LineUpsDto lineUpsDto)
        {
            await simulatorService.SendLineups(lineUpsDto);
            triggerJobService.TriggerFetchLiveMatchTimeline();
        }

        public async Task ApplyH2H(H2HDto h2HDto)
        {
            await simulatorService.SendH2H(h2HDto);
            triggerJobService.TriggerCreateMatchJobs();
        }

        public async Task<H2HDto> GenerateH2H(string matchId)
        {
            var h2hDto = await h2HService.GenerateH2HData(matchId);

            return h2hDto;
        }
    }
}
using System;
using Fanex.Logging;
using Score247.Management.Site.Models.Events;

namespace Score247.Management.Site.Services
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http;
    using Criterias.Scenarios;
    using Helpers;
    using Models.DataMappers;
    using Repositories.Matches;
    using Repositories.Scenarios;
    using Score247Management.Core.Matches.Models;
    using Score247Management.Core.Scenarios.Models;
    using Score247.Management.Site.Models;

    public interface IScenarioService
    {
        Task CreateScenario(string matchId, IFormCollection collection);

        Task ApplyScenario(string matchId, IFormCollection collection, int delay = 500);

        Task<Scenario> RandomScenario(string matchId, int delay = 2000, bool limitedEvent = false);

        Task<IEnumerable<Scenario>> GetAllScenarios();
    }

    public class ScenarioService : IScenarioService
    {
        private readonly ISimulatorService simulatorService;
        private readonly ITimelineService timelineService;
        private readonly ITriggerJobService triggerJobService;
        private readonly IMatchFileService matchFileService;
        private readonly IQueryService queryService;
        private readonly ILogger logger;

        public ScenarioService(ISimulatorService simulatorService,
            ITimelineService timelineService,
            ITriggerJobService triggerJobService,
            IMatchFileService matchFileService,
            IQueryService queryService,
            ILogger logger)
        {
            this.simulatorService = simulatorService;
            this.timelineService = timelineService;
            this.triggerJobService = triggerJobService;
            this.matchFileService = matchFileService;
            this.queryService = queryService;
            this.logger = logger;
        }

        public async Task ApplyScenario(string matchId, IFormCollection collection, int delay = 500)
        {
            try
            {
                var matchRepo = new MatchRepository();
                var scenarioRepo = new ScenarioRepository();
                var match = await matchRepo.GetMatchById(matchId);
                var scenario = await scenarioRepo.GetScenario(int.Parse(collection["scenarioId"]));
                var matchLineups = await simulatorService.GetLineUpsDto(matchId);

                await ProcessScenario(match, scenario, matchLineups, delay);

                await triggerJobService.TriggerFetchLiveMatchTimeline();
                await Task.Delay(2000);
            }
            catch (Exception ex)
            {
                logger.Error(ex.InnerException?.Message, "Apply Scenarios");
            }
        }

        public async Task<Scenario> RandomScenario(string matchId, int delay = 2000, bool limitedEvent = false)
        {
            var ran = new Random();
            var scenarioRepo = new ScenarioRepository();
            var matchRepo = new MatchRepository();
            var scenarios = (await scenarioRepo.GetScenarios()).ToList();
            var match = await matchRepo.GetMatchById(matchId);
            var scenario = scenarios[ran.Next(scenarios.Count)];
            var matchLineups = await simulatorService.GetLineUpsDto(matchId);
            if (limitedEvent)
            {
                var events = scenario.ScenarioEvents.ToList();
                scenario.ScenarioEvents = events.GetRange(0, ran.Next(1, events.Count - 1));
            }
            await ProcessScenario(match, scenario, matchLineups, delay);

            await triggerJobService.TriggerFetchLiveMatchTimeline();

            await Task.Delay(2000);

            return scenario;
        }

        private async Task ProcessScenario(Match match, Scenario scenario, LineUpsDto matchLineups = null, int delay = 2000)
        {
            foreach (var scenarioEvent in scenario.ScenarioEvents)
            {
                var preEvent = SportEventMapper.MapEvent(scenarioEvent, match, scenario, matchLineups);

                await simulatorService.SendMatchEvent(preEvent, delay);

                var content = await matchFileService.ReadLiveMatchFromFile() ?? new DailyResultDto
                {
                    generated_at = DateTime.UtcNow,
                    schema = "Matches",
                    results = new List<Result>()
                };
                var newLiveMatches = content.results.Where(x => x.sport_event.id != match.Id).ToList();
                var liveMatch = BuildLiveMatchFromMatch(match, preEvent);

                newLiveMatches.Add(liveMatch);
                content.results = newLiveMatches;
                await timelineService.SendMatchTimeline(match.Id);
                matchFileService.SaveLiveMatchToFile(content);
                simulatorService.SendLiveResult(content);
            }
        }

        private Result BuildLiveMatchFromMatch(Match match, MatchPushEventDto eventDto)
        {
            var result = new Result
            {
                sport_event = SportEventMapper.MapSportEvent(match),
                sport_event_status = eventDto.payload.sport_event_status
            };

            return result;
        }

        public async Task CreateScenario(string matchId, IFormCollection collection)
        {
            var matchRepo = new MatchRepository();
            var scenarioRepo = new ScenarioRepository();
            var match = await matchRepo.GetMatchById(matchId);
            var timelines = (await matchRepo.GetMatchTimelines(matchId)).ToList();

            var id = UniqueGenerator.GetIntId();
            var homeId = match.Teams.FirstOrDefault(x => x.IsHome)?.Id;
            var awayId = match.Teams.FirstOrDefault(x => !x.IsHome)?.Id;
            var criteria = new CreateScenarioCriteria() { Id = id, Name = collection["scenarioName"], AwayId = awayId, HomeId = homeId, EventDate = match.EventDate };
            await scenarioRepo.CreateNewScenario(criteria);
            for (var i = 0; i < timelines.Count; i++)
            {
                var timeline = timelines.ElementAt(i);
                await scenarioRepo.AddEventToScenario(id, i + 1, timeline.RequestLog);
            }
        }

        public async Task<IEnumerable<Scenario>> GetAllScenarios()
        {
            var scenarioRepo = new ScenarioRepository();
            var scenarios = await scenarioRepo.GetScenarios();

            return scenarios;
        }
    }
}
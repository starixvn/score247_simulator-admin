using System;

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

    public interface IScenarioService
    {
        Task CreateScenario(string matchId, IFormCollection collection);

        Task ApplyScenario(string matchId, IFormCollection collection, int delay = 2000);

        Task<Scenario> RandomScenario(string matchId, int delay = 2000);

        Task<IEnumerable<Scenario>> GetAllScenarios();
    }

    public class ScenarioService : IScenarioService
    {
        private readonly ISimulatorService simulatorService;
        private readonly ITimelineService timelineService;

        public ScenarioService(ISimulatorService simulatorService, ITimelineService timelineService)
        {
            this.simulatorService = simulatorService;
            this.timelineService = timelineService;
        }

        public async Task ApplyScenario(string matchId, IFormCollection collection, int delay = 2000)
        {
            var matchRepo = new MatchRepository();
            var scenarioRepo = new ScenarioRepository();
            var match = await matchRepo.GetMatchById(matchId);
            var scenario = await scenarioRepo.GetScenario(int.Parse(collection["scenarioId"]));

            await ProcessScenario(match, scenario, delay);
            await timelineService.SendUpdateTimelines(match, scenario);
        }

        public async Task<Scenario> RandomScenario(string matchId, int delay = 2000)
        {
            var ran = new Random();
            var scenarioRepo = new ScenarioRepository();
            var matchRepo = new MatchRepository();
            var scenarios = (await scenarioRepo.GetScenarios()).ToList();
            var match = await matchRepo.GetMatchById(matchId);
            var scenario = scenarios[ran.Next(0, scenarios.Count - 1)];

            await ProcessScenario(match, scenario, delay);
            await timelineService.SendUpdateTimelines(match, scenario);

            return scenario;
        }

        private async Task ProcessScenario(Match match, Scenario scenario, int delay = 2000)
        {
            foreach (var scenarioEvent in scenario.ScenarioEvents)
            {
                var preEvent = SportEventMapper.MapEvent(scenarioEvent, match, scenario);

                await simulatorService.SendMatchEvent(preEvent, delay);
            }
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
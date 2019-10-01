using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Score247.Management.Site.Criterias.Scenarios;
using Score247.Management.Site.Helpers;
using Score247.Management.Site.Models.Events;
using Score247.Management.Site.Repositories.Matches;
using Score247.Management.Site.Repositories.Scenarios;
using Score247Management.Core.Scenarios.Models;

namespace Score247.Management.Site.Services
{
    public interface IScenarioService
    {
        Task CreateScenario(string matchId, IFormCollection collection);

        Task ApplyScenario(string matchId, IFormCollection collection);

        Task<IEnumerable<Scenario>> GetAllScenarios();
    }

    public class ScenarioService : IScenarioService
    {
        private readonly ISimulatorService simulatorService;

        public ScenarioService(ISimulatorService simulatorService)
        {
            this.simulatorService = simulatorService;
        }

        public async Task ApplyScenario(string matchId, IFormCollection collection)
        {
            var matchRepo = new MatchRepository();
            var scenarioRepo = new ScenarioRepository();
            var match = await matchRepo.GetMatchById(matchId);
            var homeId = match.Teams.FirstOrDefault(x => x.IsHome).Id;
            var awayId = match.Teams.FirstOrDefault(x => !x.IsHome).Id;
            var scenario = await scenarioRepo.GetScenario(int.Parse(collection["scenarioId"]));
            var differTime = match.EventDate - scenario.EventDate;
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

                await simulatorService.SendMatchEvent(preEvent);
            }
        }

        public async Task CreateScenario(string matchId, IFormCollection collection)
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
        }

        public async Task<IEnumerable<Scenario>> GetAllScenarios()
        {
            var scenarioRepo = new ScenarioRepository();
            var scenarios = await scenarioRepo.GetScenarios();

            return scenarios;
        }
    }
}
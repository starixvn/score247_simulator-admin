namespace Score247.Management.Site.Services
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http;
    using Newtonsoft.Json;
    using Criterias.Scenarios;
    using Helpers;
    using Models.DataMappers;
    using Models.Events;
    using Repositories.Matches;
    using Repositories.Scenarios;
    using Score247Management.Core.Matches.Models;
    using Score247Management.Core.Scenarios.Models;

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
            var scenario = await scenarioRepo.GetScenario(int.Parse(collection["scenarioId"]));
            foreach (var scenarioEvent in scenario.ScenarioEvents)
            {
                var preEvent = BuildEvent(scenarioEvent, match, scenario);

                await simulatorService.SendMatchEvent(preEvent);
            }

            await SendUpdateTimelines(match, scenario);
        }

        private static MatchPushEventDto BuildEvent(ScenarioEvent scenarioEvent, Match match, Scenario scenario)
        {
            var homeId = match.Teams.FirstOrDefault(x => x.IsHome)?.Id;
            var awayId = match.Teams.FirstOrDefault(x => !x.IsHome)?.Id;
            var differTime = match.EventDate - scenario.EventDate;
            var preEvent = JsonConvert.DeserializeObject<MatchPushEventDto>(scenarioEvent.EventRequest);
            preEvent.metadata.sport_event_id = match.Id;
            preEvent.metadata.tournament_id = match.League.Id;
            preEvent.payload.timeline.time = preEvent.payload.timeline.time + differTime;
            if (preEvent.payload.sport_event_status.aggregate_winner_id == scenario.HomeId) preEvent.payload.sport_event_status.aggregate_winner_id = homeId;
            if (preEvent.payload.sport_event_status.winner_id == scenario.HomeId) preEvent.payload.sport_event_status.winner_id = homeId;
            if (preEvent.payload.sport_event_status.aggregate_winner_id == scenario.AwayId) preEvent.payload.sport_event_status.aggregate_winner_id = awayId;
            if (preEvent.payload.sport_event_status.winner_id == scenario.AwayId) preEvent.payload.sport_event_status.winner_id = awayId;

            return preEvent;
        }

        private async Task SendUpdateTimelines(Match match, Scenario scenario)
        {
            var matchTimelineDto = BuildMatchTimelineDto(match, scenario);

            await simulatorService.SendMatchTimelines(matchTimelineDto);
        }

        private MatchTimelineDto BuildMatchTimelineDto(Match match, Scenario scenario)
        {
            var matchEvents = scenario.ScenarioEvents.Select(scenarioEvent => BuildEvent(scenarioEvent, match, scenario)).ToList();
            var latestEvents = matchEvents.LastOrDefault();
            var matchTimelineDto = new MatchTimelineDto
            {
                generated_at = scenario.EventDate,
                schema = "http://schemas.sportradar.com/bsa/soccer/v1/json/endpoints/soccer/match_timeline.json",
                sport_event = SportEventMapper.MapSportEvent(match),
                sport_event_status = latestEvents?.payload.sport_event_status,
                timeline = matchEvents.Select(matchEvent => matchEvent.payload.timeline)
            };

            return matchTimelineDto;
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
            for (int i = 0; i < timelines.Count; i++)
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
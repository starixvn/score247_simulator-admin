using Score247.Management.Site.Models.DataMappers;
using Score247.Management.Site.Models.Events;

namespace Score247.Management.Site.Services
{
    using System.Linq;
    using System.Threading.Tasks;
    using Score247Management.Core.Matches.Models;
    using Score247Management.Core.Scenarios.Models;

    public interface ITimelineService
    {
        Task SendUpdateTimelines(Match match, Scenario scenario);
    }

    public class TimelineService : ITimelineService
    {
        private readonly ISimulatorService simulatorService;

        public TimelineService(ISimulatorService simulatorService)
        {
            this.simulatorService = simulatorService;
        }

        public async Task SendUpdateTimelines(Match match, Scenario scenario)
        {
            var matchTimelineDto = BuildMatchTimelineDto(match, scenario);

            await simulatorService.SendMatchTimelines(matchTimelineDto);
        }

        private MatchTimelineDto BuildMatchTimelineDto(Match match, Scenario scenario)
        {
            var matchEvents = scenario.ScenarioEvents.Select(scenarioEvent => SportEventMapper.MapEvent(scenarioEvent, match, scenario)).ToList();
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
    }
}
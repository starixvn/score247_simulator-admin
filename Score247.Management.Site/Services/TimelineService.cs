using Score247.Management.Site.Models.DataMappers;
using Score247.Management.Site.Models.Events;
using Score247.Management.Site.Repositories.Matches;
using Score247.Management.Site.Repositories.Matches.Models;

namespace Score247.Management.Site.Services
{
    using System.Linq;
    using System.Threading.Tasks;
    using Score247Management.Core.Matches.Models;
    using Score247Management.Core.Scenarios.Models;

    public interface ITimelineService
    {
        Task SendUpdateTimelines(Match match, Scenario scenario);

        Task SendInitTimeline(MatchEntity match);

        Task SendMatchTimeline(string matchId);
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

        public async Task SendInitTimeline(MatchEntity match)
        {
            var matchTimelineDto = BuildMatchInitTimelineDto(match);

            await simulatorService.SendMatchTimelines(matchTimelineDto);
        }

        public async Task SendMatchTimeline(string matchId)
        {
            var matchRepository = new MatchRepository();
            var timelines = (await matchRepository.GetMatchTimelinesLog(matchId)).ToList();
            var latestTimeline = timelines.LastOrDefault();
            var match = await matchRepository.GetMatchById(matchId);

            var matchTimelineDto = new MatchTimelineDto
            {
                generated_at = match.EventDate,
                schema = "http://schemas.sportradar.com/bsa/soccer/v1/json/endpoints/soccer/match_timeline.json",
                sport_event = SportEventMapper.MapSportEvent(match),
                sport_event_status = latestTimeline?.payload.sport_event_status,
                timeline = timelines.Select(x => x.payload.timeline)
            };

            await simulatorService.SendMatchTimelines(matchTimelineDto);
        }

        private static MatchTimelineDto BuildMatchTimelineDto(Match match, Scenario scenario)
        {
            var matchEvents = scenario.ScenarioEvents.Select(scenarioEvent => SportEventMapper.MapEvent(scenarioEvent, match, scenario)).ToList();
            var latestEvents = matchEvents.LastOrDefault();
            var matchTimelineDto = new MatchTimelineDto
            {
                generated_at = match.EventDate,
                schema = "http://schemas.sportradar.com/bsa/soccer/v1/json/endpoints/soccer/match_timeline.json",
                sport_event = SportEventMapper.MapSportEvent(match),
                sport_event_status = latestEvents?.payload.sport_event_status,
                timeline = matchEvents.Select(matchEvent => matchEvent.payload.timeline)
            };

            return matchTimelineDto;
        }

        private static MatchTimelineDto BuildMatchInitTimelineDto(MatchEntity match)
        {
            var matchTimelineDto = new MatchTimelineDto
            {
                generated_at = match.Scheduled,
                schema = "http://schemas.sportradar.com/bsa/soccer/v1/json/endpoints/soccer/match_timeline.json",
                sport_event = SportEventMapper.MapSportEvent(match),
                sport_event_conditions = new Sport_Event_Conditions()
                {
                    venue = SportEventMapper.MapVenueDto(match.Venue),
                    referee = new Referee()
                    {
                        name = match.Referee
                    }
                },
                sport_event_status = new Sport_Event_Status()
                {
                    status = "not_started"
                },
                timeline = null
            };

            return matchTimelineDto;
        }
    }
}
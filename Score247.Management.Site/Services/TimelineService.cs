using System;
using System.Collections.Generic;
using Score247.Management.Site.Models.DataMappers;
using Score247.Management.Site.Models.Events;
using Score247.Management.Site.Repositories.Matches;
using Score247.Management.Site.Repositories.Matches.Models;
using Score247.Management.Site.Services.Matches.Creators;
using Score247Management.Core._Shared.Enumerations;
using Score247Management.Core.Statistics.Models;

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

        Task ApplyStatistic(Statistic statistic);
    }

    public class TimelineService : ITimelineService
    {
        private readonly ISimulatorService simulatorService;
        private readonly ITriggerJobService triggerJobService;

        public TimelineService(ISimulatorService simulatorService, ITriggerJobService triggerJobService)
        {
            this.simulatorService = simulatorService;
            this.triggerJobService = triggerJobService;
        }

        public async Task SendUpdateTimelines(Match match, Scenario scenario)
        {
            var matchTimelineDto = BuildMatchTimelineDto(match, scenario);

            await simulatorService.SendMatchTimelines(matchTimelineDto);
            await triggerJobService.TriggerFetchLiveMatchTimeline();

            await Task.Delay(2000);
        }

        public async Task SendInitTimeline(MatchEntity match)
        {
            var matchTimelineDto = BuildMatchInitTimelineDto(match);

            await simulatorService.SendMatchTimelines(matchTimelineDto);
            await triggerJobService.TriggerFetchLiveMatchTimeline();
        }

        public async Task SendMatchTimeline(string matchId)
        {
            var matchTimelineDto = await BuildCurrentMatchTimeline(matchId);

            await simulatorService.SendMatchTimelines(matchTimelineDto);
            await triggerJobService.TriggerFetchLiveMatchTimeline();

            await Task.Delay(2000);
        }

        private async Task<MatchTimelineDto> BuildCurrentMatchTimeline(string matchId)
        {
            var matchRepository = new MatchRepository();
            var timelineLogs = (await matchRepository.GetMatchTimelinesLog(matchId)).ToList();
            var latestTimeline = timelineLogs.LastOrDefault();
            var match = await matchRepository.GetMatchById(matchId);

            var matchTimelineDto = new MatchTimelineDto
            {
                generated_at = match.EventDate,
                schema = "http://schemas.sportradar.com/bsa/soccer/v1/json/endpoints/soccer/match_timeline.json",
                sport_event = SportEventMapper.MapSportEvent(match),
                sport_event_status = latestTimeline?.payload.sport_event_status,
                timeline = timelineLogs.Select(x => x.payload.timeline).ToList()
            };

            matchTimelineDto.statistics = BuildStatisticFormTimelineDto(matchTimelineDto);

            return matchTimelineDto;
        }

        private Statistics BuildStatisticFormCustomStatistic(Statistic customStatistic, Statistics statistics)
        {
            var newStatistics = statistics.teams.Select(team =>
            {
                team.statistics = StatisticMapper.MapStatistic(team.qualifier == "home" ? customStatistic.HomeStatistic : customStatistic.AwayStatistic);

                return team;
            });
            statistics.teams = newStatistics;

            return statistics;
        }

        public async Task ApplyStatistic(Statistic statistic)
        {
            var matchNotStarted = await CheckMatchNotStart(statistic.MatchId);
            if (matchNotStarted)
            {
                await SendStartMatch(statistic.MatchId);
            }

            var matchTimelineDto = await BuildCurrentMatchTimeline(statistic.MatchId);

            matchTimelineDto.statistics = BuildStatisticFormCustomStatistic(statistic, matchTimelineDto.statistics);

            await simulatorService.SendMatchTimelines(matchTimelineDto);
            await triggerJobService.TriggerFetchLiveMatchTimeline();
        }

        private async Task<bool> CheckMatchNotStart(string matchId)
        {
            var matchRepository = new MatchRepository();
            var match = await matchRepository.GetMatchById(matchId);

            return match.MatchResult.MatchStatus.DisplayName == "not_started";
        }

        private async Task SendStartMatch(string matchId)
        {
            var matchRepository = new MatchRepository();
            var match = await matchRepository.GetMatchById(matchId);
            var matchStartEvent = MatchTimeEventCreator.CreateMatchStart(match, match.EventDate);
            await simulatorService.SendMatchEvent(matchStartEvent, 1000);
        }

        private static Statistics BuildStatisticFormTimelineDto(MatchTimelineDto timelineDto)
        {
            var statistic = new Statistics
            {
                teams = timelineDto.sport_event.competitors.Select(InitTeamStatistic)
                    .Select(team => BuildTeamStatistic(team, timelineDto.timeline)).ToList()
            };

            var ballPossession = new Random().Next(10, 90);
            statistic.teams.ElementAt(0).statistics.ball_possession = ballPossession;
            statistic.teams.ElementAt(1).statistics.ball_possession =
                100 - statistic.teams.ElementAt(0).statistics.ball_possession;

            return statistic;
        }

        private static Team BuildTeamStatistic(Team team, IEnumerable<TimelineDto> timelines)
        {
            var teamTimelines = timelines.Where(tl => tl.team == team.qualifier).ToList();

            foreach (var timeline in teamTimelines)
            {
                if (timeline.type == EventType.CornerKick.DisplayName)
                {
                    team.statistics.corner_kicks++;
                }
                else if (timeline.type == EventType.FreeKick.DisplayName)
                {
                    team.statistics.free_kicks++;
                }
                else if (timeline.type == EventType.GoalKick.DisplayName)
                {
                    team.statistics.goal_kicks++;
                }
                else if (timeline.type == EventType.Injury.DisplayName)
                {
                    team.statistics.injuries++;
                }
                else if (timeline.type == EventType.Offside.DisplayName)
                {
                    team.statistics.offsides++;
                }
                else if (timeline.type == EventType.ShotOffTarget.DisplayName)
                {
                    team.statistics.shots_off_target++;
                }
                else if (timeline.type == EventType.ShotOnTarget.DisplayName)
                {
                    team.statistics.shots_on_target++;
                }
                else if (timeline.type == EventType.ShotSaved.DisplayName)
                {
                    team.statistics.shots_saved++;
                }
                else if (timeline.type == EventType.ThrowIn.DisplayName)
                {
                    team.statistics.throw_ins++;
                }
                else if (timeline.type == EventType.YellowCard.DisplayName)
                {
                    team.statistics.yellow_cards++;
                }
                else if (timeline.type == EventType.YellowRedCard.DisplayName)
                {
                    team.statistics.yellow_red_cards++;
                }
                else if (timeline.type == EventType.RedCard.DisplayName)
                {
                    team.statistics.red_cards++;
                }
            }

            team.statistics.fouls++;
            team.statistics.shots_blocked++;

            return team;
        }

        private static Team InitTeamStatistic(Competitor competitor)
        {
            var team = new Team()
            {
                id = competitor.id,
                name = competitor.name,
                qualifier = competitor.qualifier,
                abbreviation = competitor.abbreviation,
                statistics = new Statistics1()
            };

            return team;
        }

        private static MatchTimelineDto BuildMatchTimelineDto(Match match, Scenario scenario)
        {
            var matchEvents = scenario.ScenarioEvents.Select(scenarioEvent => SportEventMapper.MapEvent(scenarioEvent, match, scenario)).ToList();
            var latestEvent = matchEvents.LastOrDefault();
            var matchTimelineDto = new MatchTimelineDto
            {
                generated_at = match.EventDate,
                schema = "http://schemas.sportradar.com/bsa/soccer/v1/json/endpoints/soccer/match_timeline.json",
                sport_event = SportEventMapper.MapSportEvent(match),
                sport_event_status = latestEvent?.payload.sport_event_status,
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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Score247.Management.Site.Criterias.League;
using Score247.Management.Site.Helpers;
using Score247.Management.Site.Models.DataMappers;
using Score247.Management.Site.Models.Dtos;
using Score247.Management.Site.Models.Events;
using Score247.Management.Site.Repositories.Leagues;
using Score247.Management.Site.Repositories.Matches;
using Score247Management.Core.Leagues.Dtos;
using Score247Management.Core.Leagues.Models;

namespace Score247.Management.Site.Services
{
    public interface ILeagueService
    {
        Task<bool> SyncSportradarLeague();

        Task EnableDisableLeague(string id);

        Task EnableMajorLeague();

        Task EnableAllLeague();

        Task ApplyMajorLeagues();

        Task<TournamentStandingDto> GetDefaultLeagueStanding(string matchId);

        Task ApplyStanding(TournamentStandingDto tournamentStandingDto);

        Task<IEnumerable<League>> GetActiveLeagues();

        Task GenerateLeagueFixture(string leagueId, TournamentScheduleDto tournamentScheduleDto);
    }

    public class LeagueService : ILeagueService
    {
        private IConfiguration configuration { get; }

        private readonly ITriggerJobService triggerJobService;
        private readonly ISimulatorFileService simulatorFileService;
        private readonly ISimulatorService simulatorService;

        public LeagueService(IConfiguration configuration, ITriggerJobService triggerJobService, ISimulatorFileService simulatorFileService, ISimulatorService simulatorService)
        {
            this.configuration = configuration;
            this.triggerJobService = triggerJobService;
            this.simulatorFileService = simulatorFileService;
            this.simulatorService = simulatorService;
        }

        public async Task<bool> SyncSportradarLeague()
        {
            await GetAndInsertGroupLeagues("other", configuration.GetValue<string>("SportradarKey:SoccerOther"));
            await GetAndInsertGroupLeagues("intl", configuration.GetValue<string>("SportradarKey:SoccerInternational"));
            await GetAndInsertGroupLeagues("as", configuration.GetValue<string>("SportradarKey:SoccerAsia"));
            await GetAndInsertGroupLeagues("am", configuration.GetValue<string>("SportradarKey:SoccerAmericas"));
            await GetAndInsertGroupLeagues("eu", configuration.GetValue<string>("SportradarKey:SoccerEurope"));

            return true;
        }

        private async Task GetAndInsertGroupLeagues(string leagueGroup, string apiKey)
        {
            var leagues = await GetLeaguesFromSportradar(leagueGroup, apiKey);

            foreach (var league in leagues)
            {
                await InseartLeague(league, leagueGroup);
            }
        }

        private async Task<IEnumerable<League>> GetLeaguesFromSportradar(string leagueGroup, string apiKey)
        {
            var requestPath = "/soccer-p3/" + leagueGroup + "/en/tournaments.json?api_key=" + apiKey;

            return await GetLeagueFromUrl(requestPath);
        }

        private async Task<IEnumerable<League>> GetLeagueFromUrl(string requestUrl)
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(configuration.GetValue<string>("SportradarUrl:Soccer"));

                var response = await client.GetAsync(requestUrl);
                var resBody = await response.Content.ReadAsStringAsync();

                LeagueSportradarList leagues = JsonConvert.DeserializeObject<LeagueSportradarList>(resBody);

                return leagues.tournaments;
            }
        }

        private async Task InseartLeague(League league, string region)
        {
            var repo = new LeagueRepository();
            var criteria = new CreateLeagueCriteria
            {
                LeagueId = league.Id,
                CategoryId = league.LeagueCategory?.Id,
                Country = league.LeagueCategory?.Name,
                CountryCode = league.LeagueCategory?.CountryCode,
                IsActive = false,
                LeagueName = league.Name,
                LeagueOrder = 1000,
                Language = "en-US",
                Region = region
            };

            await repo.CreateLeague(criteria);
        }

        public async Task EnableDisableLeague(string id)
        {
            var leagueRepository = new LeagueRepository();
            var league = await leagueRepository.GetLeagueById(id);
            var command = new UpdateActiveStatusCommand { IsActive = !league.IsActive, LeagueId = id };
            await leagueRepository.UpdateLeagueActiveStatus(command);
            await triggerJobService.TriggerFetchCleanMajorLeagueCache();
            if (league.IsActive)
            {
                var removeSeasonCommand = new RemoveLeagueSeasonCommand(id);
                await leagueRepository.RemoveLeagueSeason(removeSeasonCommand);
            }
            await triggerJobService.TriggerFetchLeaguesSeason();
        }

        public async Task EnableMajorLeague()
        {
            var leagueRepository = new LeagueRepository();
            var command = new EnableMajorLeaguesCommand();

            await leagueRepository.EnableMajorLeagues(command);
            await triggerJobService.TriggerFetchCleanMajorLeagueCache();
        }

        public async Task EnableAllLeague()
        {
            var leagueRepository = new LeagueRepository();
            var command = new EnableAllLeaguesCommand();

            await leagueRepository.EnableAllLeagues(command);
            await triggerJobService.TriggerFetchCleanMajorLeagueCache();
        }

        public async Task ApplyMajorLeagues()
        {
            await triggerJobService.TriggerCreateMatchJobs();
        }

        public async Task<TournamentStandingDto> GetDefaultLeagueStanding(string matchId)
        {
            var matchRepository = new MatchRepository();
            var standing = await simulatorFileService.ReadDefaultStanding();
            var match = await matchRepository.GetMatchById(matchId);
            if (match == null)
            {
                return standing;
            }

            standing.tournament = SportEventMapper.MapTournament(match.League);
            standing.tournament.current_season = SportEventMapper.MapSeason(match.LeagueSeason);
            standing.season = SportEventMapper.MapSeason(match.LeagueSeason);
            var rand = new Random();

            foreach (var groupStanding in standing.standings)
            {
                var groups = groupStanding.groups.ToList();
                var group = groups[rand.Next(groups.Count)];

                var teams = group.team_standings.ToList();
                var posHome = rand.Next(teams.Count);
                var posAway = rand.Next(teams.Count);
                while (posAway == posHome)
                {
                    posAway = rand.Next(teams.Count);
                }

                teams[posHome].team.id = match.Teams.ElementAt(0).Id;
                teams[posHome].team.name = match.Teams.ElementAt(0).Name;

                teams[posAway].team.id = match.Teams.ElementAt(1).Id;
                teams[posAway].team.name = match.Teams.ElementAt(1).Name;
            }

            return standing;
        }

        public async Task ApplyStanding(TournamentStandingDto tournamentStandingDto)
        {
            await simulatorService.SendTournamentStanding(tournamentStandingDto);
            await triggerJobService.TriggerFetchLiveMatch();
        }

        public Task<IEnumerable<League>> GetActiveLeagues()
        {
            var leagueRepository = new LeagueRepository();

            return leagueRepository.GetActiveLeagues();
        }

        public async Task GenerateLeagueFixture(string leagueId, TournamentScheduleDto tournamentScheduleDto)
        {
            var leagueRepository = new LeagueRepository();
            var league = await leagueRepository.GetLeagueById(leagueId);
            var tournament = SportEventMapper.MapTournament(league);
            var schedule = tournamentScheduleDto;
            if (schedule == null)
            {
                schedule = await simulatorFileService.GetTournamentSchedule();
            }

            schedule.generated_at = DateTime.Now;
            schedule.tournament.id = tournament.id;
            schedule.tournament.name = tournament.name;
            foreach (var sportEveht in schedule.sport_events)
            {
                sportEveht.tournament.id = tournament.id;
                sportEveht.tournament.name = tournament.name;
                sportEveht.season.tournament_id = tournament.id;
                sportEveht.id = UniqueGenerator.GetIdWithPattern("sr:match:");

                if (sportEveht.status == "closed")
                {
                    await SendTimelineForMatch(sportEveht);
                }
                else
                {
                    await SendNotStartTimelineForMatch(sportEveht);
                }
            }

            await simulatorService.SendTournamentSchedule(schedule);
            await triggerJobService.TriggerFetchLeagueMatches();
        }

        private async Task SendNotStartTimelineForMatch(Sport_Event sportEveht)
        {
            var timeline = await simulatorFileService.GetNotStartTimelime();

            var sportTimeline = TimelineMapper.MapTimelinetoSportEvent(sportEveht, timeline);

            await simulatorService.SendMatchTimelines(sportTimeline);
        }

        private async Task SendTimelineForMatch(Sport_Event sportEveht)
        {
            var timeline = await simulatorFileService.GetRandomTimelime();

            var sportTimeline = TimelineMapper.MapTimelinetoSportEvent(sportEveht, timeline);

            await simulatorService.SendMatchTimelines(sportTimeline);
        }
    }
}
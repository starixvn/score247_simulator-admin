using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Score247.Management.Site.Criterias.League;
using Score247.Management.Site.Repositories.Leagues;
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
    }

    public class LeagueService : ILeagueService
    {
        private IConfiguration configuration { get; }
        private readonly ITriggerJobService triggerJobService;

        public LeagueService(IConfiguration configuration, ITriggerJobService triggerJobService)
        {
            this.configuration = configuration;
            this.triggerJobService = triggerJobService;
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
            var criteria = new CreateLeagueCriteria()
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
            await ApplyMajorLeagues();
        }

        public async Task EnableMajorLeague()
        {
            var leagueRepository = new LeagueRepository();
            var command = new EnableMajorLeaguesCommand();

            await leagueRepository.EnableMajorLeagues(command);
            await ApplyMajorLeagues();
        }

        public async Task EnableAllLeague()
        {
            var leagueRepository = new LeagueRepository();
            var command = new EnableAllLeaguesCommand();

            await leagueRepository.EnableAllLeagues(command);
            ApplyMajorLeagues();
        }

        private async Task ApplyMajorLeagues()
        {
            await triggerJobService.TriggerFetchCleanMajorLeagueCache();
            await Task.Delay(10000);
            await triggerJobService.TriggerCreateMatchJobs();
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
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
    }

    public class LeagueService : ILeagueService
    {
        private IConfiguration configuration { get; }

        public LeagueService(IConfiguration configuration)
        {
            this.configuration = configuration;
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
                var leagueDetail = await GetLeagueDetailInfo(leagueGroup, apiKey, league.Id);
                if (leagueDetail.tournament == null)
                {
                    leagueDetail.tournament = league;
                }
                await InseartLeagueSeason(leagueDetail);
                await InseartLeagueCategory(leagueDetail);
                await InseartLeague(leagueDetail, leagueGroup);
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

        private async Task<LeagueSportradarInfo> GetLeagueDetailInfo(string leagueGroup, string apiKey, string tournamentId)
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(configuration.GetValue<string>("SportradarUrl:Soccer"));
                var requestUrl = "/soccer-p3/" + leagueGroup + "/en/tournaments/" + tournamentId + "/info.json?api_key=" + apiKey;
                var response = await client.GetAsync(requestUrl);
                var resBody = await response.Content.ReadAsStringAsync();

                LeagueSportradarInfo league = JsonConvert.DeserializeObject<LeagueSportradarInfo>(resBody);

                return league;
            }
        }

        private async Task InseartLeagueSeason(LeagueSportradarInfo league)
        {
            var repo = new LeagueRepository();
            try
            {
                var criteria = new CreateLeagueSeasonCriteria()
                {
                    SeasonId = league.tournament.LeagueSeason?.Id,
                    SeasonName = league.tournament.LeagueSeason?.Name,
                    StartDate = league.tournament.LeagueSeason?.StartDate != null ? league.tournament.LeagueSeason.StartDate.UtcDateTime : DateTime.MinValue,
                    EndDate = league.tournament.LeagueSeason?.EndDate != null ? league.tournament.LeagueSeason.EndDate.UtcDateTime : DateTime.MinValue,
                    SeasonYear = league.tournament.LeagueSeason?.Year,
                    TournamentId = league.tournament.Id
                };

                await repo.CreateLeagueSeason(criteria);
            }
            catch (Exception ex)
            {
            }
        }

        private async Task InseartLeagueCategory(LeagueSportradarInfo league)
        {
            try
            {
                var repo = new LeagueRepository();
                var criteria = new CreateLeagueCategoryCriteria()
                {
                    CategoryId = league.tournament.LeagueCategory.Id,
                    CategoryName = league.tournament.LeagueCategory.Name,
                    CategoryCode = league.tournament.LeagueCategory.CountryCode
                };

                await repo.CreateLeageCategory(criteria);
            }
            catch (Exception ex)
            {
            }
        }

        private async Task InseartLeague(LeagueSportradarInfo league, string region)
        {
            try
            {
                var repo = new LeagueRepository();
                var criteria = new CreateLeagueCriteria()
                {
                    LeagueId = league.tournament.Id,
                    CategoryId = league.tournament.LeagueCategory?.Id,
                    Country = league.tournament.LeagueCategory?.Name,
                    CountryCode = league.tournament.LeagueCategory?.CountryCode,
                    GroupValue = JsonConvert.SerializeObject(league.groups),
                    IsActive = false,
                    LeagueName = league.tournament.Name,
                    LeagueOrder = 1000,
                    RoundNumber = league.round != null ? league.round.number : 0,
                    RoundType = league.round?.type,
                    SeasonId = league.tournament.LeagueSeason?.Id,
                    Language = "en-US",
                    Region = region
                };

                await repo.CreateLeague(criteria);
            }
            catch (Exception ex)
            {
            }
        }
    }
}
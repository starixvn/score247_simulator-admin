using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fanex.Data;
using Fanex.Data.Repository;
using Score247.Management.Site.Criterias.League;
using Score247.Management.Site.Helpers;
using Score247.Management.Site.Repositories.Images;
using Score247Management.Core.Leagues.Models;

namespace Score247.Management.Site.Repositories.Leagues
{
    public class LeagueRepository
    {
        private readonly IDynamicRepository repository;

        public LeagueRepository()
        {
            repository = new DynamicRepository();
        }

        public async Task<IEnumerable<League>> GetLeagues()
        {
            var criteria = new GetLeagueCriteria();
            var leagues = await repository.FetchAsync<League>(criteria);
            var leagueCategorys = await GetLeagueCategory();

            foreach (var league in leagues)
            {
                var leagueCategory = leagueCategorys.FirstOrDefault(x => x.CountryCode == league.CountryCode);
                if (leagueCategory == null)
                {
                    league.LeagueCategory = new LeagueCategory()
                    {
                        Name = league.Country
                    };
                }
                else
                {
                    league.LeagueCategory = leagueCategory;
                }
            }

            return leagues;
        }

        public async Task<IEnumerable<League>> GetActiveLeagues()
        {
            var leagues = await GetLeagues();
            var leagueCategorys = await GetLeagueCategory();

            foreach (var league in leagues)
            {
                var leagueCategory = leagueCategorys.FirstOrDefault(x => x.CountryCode == league.CountryCode);
                if (leagueCategory == null)
                {
                    league.LeagueCategory = new LeagueCategory()
                    {
                        Name = league.Country
                    };
                }
                else
                {
                    league.LeagueCategory = leagueCategory;
                }
            }
            return leagues.Where(x => x.IsActive);
        }

        public async Task<League> GetLeagueById(string id)
        {
            try
            {
                var league = await repository.GetAsync<League>(new GetLeagueByIdCriteria() { LeagueId = id });
                var leagueCategorys = await GetLeagueCategory();
                var leagueCategory = leagueCategorys.FirstOrDefault(x => x.CountryCode == league.CountryCode);
                if (leagueCategory == null)
                {
                    league.LeagueCategory = new LeagueCategory()
                    {
                        Name = league.Country
                    };
                }
                else
                {
                    league.LeagueCategory = leagueCategory;
                }

                return league;
            }
            catch (System.Exception ex)
            {
                throw;
            }
        }

        public async Task<IEnumerable<LeagueCategory>> GetLeagueCategory()
        {
            return await repository.FetchAsync<LeagueCategory>(new GetLeagueCategoryCriteria());
        }

        public async Task<int> CreateLeague(CreateLeagueCriteria criteria)
        {
            return await repository.ExecuteAsync(criteria);
        }

        public async Task<int> CreateLeagueSeason(CreateLeagueSeasonCriteria criteria)
        {
            return await repository.ExecuteAsync(criteria);
        }

        public async Task<int> CreateLeageCategory(CreateLeagueCategoryCriteria criteria)
        {
            return await repository.ExecuteAsync(criteria);
        }
    }
}
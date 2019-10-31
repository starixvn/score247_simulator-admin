using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fanex.Data.Repository;
using Score247.Management.Site.Criterias.League;
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
                var leagueCategory = leagueCategorys.FirstOrDefault(x => x.Id == league.CategoryId);
                if (leagueCategory == null)
                {
                    league.LeagueCategory = new LeagueCategory()
                    {
                        Name = league.Country,
                        CountryCode = league.CountryCode
                    };
                }
                else
                {
                    league.LeagueCategory = leagueCategory;
                    if (string.IsNullOrWhiteSpace(league.LeagueCategory.CountryCode))
                    {
                        league.LeagueCategory.CountryCode = league.CountryCode;
                    }
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
                var leagueCategory = leagueCategorys.FirstOrDefault(x => x.Id == league.CategoryId);
                if (leagueCategory == null)
                {
                    league.LeagueCategory = new LeagueCategory
                    {
                        Name = league.Country,
                        CountryCode = league.CountryCode
                    };
                }
                else
                {
                    league.LeagueCategory = leagueCategory;
                    if (string.IsNullOrWhiteSpace(league.LeagueCategory.CountryCode))
                    {
                        league.LeagueCategory.CountryCode = league.CountryCode;
                    }
                }
            }
            return leagues.Where(x => x.IsActive);
        }

        public async Task<League> GetLeagueById(string id)
        {
            var league = await repository.GetAsync<League>(new GetLeagueByIdCriteria() { LeagueId = id });
            var leagueCategorys = await GetLeagueCategory();
            var leagueCategory = leagueCategorys.FirstOrDefault(x => x.Id == league.CategoryId);
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
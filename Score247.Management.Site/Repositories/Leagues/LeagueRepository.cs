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
            return await repository.FetchAsync<League>(criteria);
        }

        public async Task<IEnumerable<League>> GetActiveLeagues()
        {
            var result = await GetLeagues();

            return result.Where(x => x.IsActive);
        }

        public async Task<Score247Management.Core.Leagues.Models.League> GetLeagueById(string id)
        {
            return (await repository.FetchAsync<Score247Management.Core.Leagues.Models.League>(new GetLeagueByIdCriteria() { Id = id })).FirstOrDefault();
        }

        public async Task<IEnumerable<Score247Management.Core.Leagues.Models.LeagueCategory>> GetLeagueCategory()
        {
            return await repository.FetchAsync<Score247Management.Core.Leagues.Models.LeagueCategory>(new GetLeagueCategoryCriteria());
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
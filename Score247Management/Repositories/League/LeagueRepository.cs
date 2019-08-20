using System.Collections.Generic;
using System.Threading.Tasks;
using Fanex.Data;
using Fanex.Data.Repository;
using Score247Management.Core.Leagues.Models;
using Score247Management.Criterias.League;

namespace Score247Management.Repositories.League
{
    public class LeagueRepository
    {
        private readonly IDynamicRepository repository;
        public LeagueRepository()
        {
            repository = new DynamicRepository();
        }

        public async Task<IEnumerable<Score247Management.Core.Leagues.Models.League>> GetLeagues()
        {
            using (var objectDb = ObjectDbFactory.CreateInstance("League_GetLeague"))
            {
                IEnumerable<Score247Management.Core.Leagues.Models.League> leagues = objectDb.Query<Score247Management.Core.Leagues.Models.League, LeagueCategory, Score247Management.Core.Leagues.Models.League>(
                  (league, category) =>
                  {
                      league.LeagueCategory = category;

                      return league;
                  }, "LeagueCategoryId");
                return leagues;
            }
            //return await repository.FetchAsync<Score247Management.Core.Leagues.Models.League>(new GetLeagueCriteria());
        }

        public async Task<IEnumerable<Score247Management.Core.Leagues.Models.LeagueCategory>> GetLeagueCategory()
        {
            return await repository.FetchAsync<Score247Management.Core.Leagues.Models.LeagueCategory>(new GetLeagueCategoryCriteria());
        }

        public async Task<int> CreateLeague(CreateLeagueCriteria criteria)
        {
            return await repository.ExecuteAsync(criteria);
        }
    }
}

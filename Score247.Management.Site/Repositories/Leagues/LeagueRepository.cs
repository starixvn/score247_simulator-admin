using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fanex.Data;
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
                return leagues.OrderBy(x => x.Name);
            }
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
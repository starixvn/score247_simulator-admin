using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fanex.Data.Repository;
using Score247.Management.Site.Criterias.Team;
using Score247Management.Core.Teams.Models;

namespace Score247.Management.Site.Repositories.Teams
{
    public class TeamRepository
    {
        private readonly IDynamicRepository repository;

        public TeamRepository()
        {
            repository = new DynamicRepository();
        }

        public async Task<IEnumerable<Team>> GetTeams()
        {
            return (await repository.FetchAsync<Team>(new GetTeamsCriteria())).OrderBy(x => x.Name);
        }

        public async Task<int> CreateTeam(CreateTeamCriteria criteria)
        {
            return await repository.ExecuteAsync(criteria);
        }

        public async Task<Team> GetTeam(GetTeamByIdCriteria criteria)
        {
            return await repository.GetAsync<Team>(criteria);
        }
    }
}
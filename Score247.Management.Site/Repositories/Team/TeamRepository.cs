using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fanex.Data.Repository;
using Score247.Management.Site.Criterias.Team;

namespace Score247.Management.Site.Repositories.Team
{
    public class TeamRepository
    {
        private readonly IDynamicRepository repository;

        public TeamRepository()
        {
            repository = new DynamicRepository();
        }

        public async Task<IEnumerable<Score247Management.Core.Teams.Models.Team>> GetTeams()
        {
            return (await repository.FetchAsync<Score247Management.Core.Teams.Models.Team>(new GetTeamsCriteria())).OrderBy(x => x.Name);
        }

        public async Task<int> CreateTeam(CreateTeamCriteria criteria)
        {
            return await repository.ExecuteAsync(criteria);
        }
    }
}

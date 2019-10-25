namespace Score247.Management.Site.Repositories.Scenarios
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Fanex.Data.Repository;
    using Score247.Management.Site.Criterias.Scenarios;
    using Score247.Management.Site.Helpers;
    using Score247Management.Core.Matches.Models;
    using Score247Management.Core.Scenarios.Models;

    public class ScenarioRepository
    {
        private readonly IDynamicRepository repository;

        public ScenarioRepository()
        {
            repository = new DynamicRepository();
        }

        public async Task<int> CreateNewScenario(CreateScenarioCriteria criteria)
        {
            return await repository.ExecuteAsync(criteria);
        }

        public async Task<int> AddEventToScenario(int scenarioId, int eventOrder, string eventRequest)
        {
            var id = UniqueGenerator.GetIntId();
            var criteria = new AddEventToScenarioCriteria() { Id = id, EventOrder = eventOrder, EventRequest = eventRequest, ScenarioId = scenarioId };
            return await repository.ExecuteAsync(criteria);
        }

        public async Task<Scenario> GetScenario(int id)
        {
            var scenario = (await repository.FetchAsync<Scenario>(new GetScenarioByIdCriteria() { Id = id })).FirstOrDefault();
            var events = await repository.FetchAsync<ScenarioEvent>(new GetScenarioEventsCriteria() { ScenarioId = id });
            scenario.ScenarioEvents = events.OrderBy(x => x.Order);
            return scenario;
        }

        public async Task<IEnumerable<Scenario>> GetScenarios()
        {
            var scenarios = await repository.FetchAsync<Scenario>(new GetAllScenariosCriteria());
            foreach (var scenario in scenarios)
            {
                scenario.ScenarioEvents = await repository.FetchAsync<ScenarioEvent>(new GetScenarioEventsCriteria() { ScenarioId = scenario.Id });
            }

            return scenarios;
        }
    }
}
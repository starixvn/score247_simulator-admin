using Fanex.Data.Repository;

namespace Score247.Management.Site.Criterias.Scenarios
{
    public class GetScenarioByIdCriteria : CriteriaBase
    {
        public int Id { get; set; }

        public override string GetSettingKey()
            => "Scenario_GetById";

        public override bool IsValid()
            => true;
    }
}
namespace Score247.Management.Site.Criterias.Scenarios
{
    using Fanex.Data.Repository;

    public class GetScenarioEventsCriteria : CriteriaBase
    {
        public int ScenarioId { get; set; }

        public override string GetSettingKey()
            => "Scenario_GetScenarioEvents";

        public override bool IsValid()
            => true;
    }
}
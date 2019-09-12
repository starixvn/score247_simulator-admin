namespace Score247.Management.Site.Criterias.Scenarios
{
    using Fanex.Data.Repository;

    public class GetAllScenariosCriteria : CriteriaBase
    {
        public override string GetSettingKey()
            => "Scenario_GetAll";

        public override bool IsValid()
            => true;
    }
}
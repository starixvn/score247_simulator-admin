namespace Score247.Management.Site.Criterias.Scenarios
{
    using Fanex.Data.Repository;

    public class AddEventToScenarioCriteria : NonQueryCommand
    {
        public int Id { get; set; }
        public int ScenarioId { get; set; }
        public int EventOrder { get; set; }
        public string EventRequest { get; set; }

        public override string GetSettingKey()
            => "Scenario_InsertTimeline";

        public override bool IsValid()
            => true;
    }
}
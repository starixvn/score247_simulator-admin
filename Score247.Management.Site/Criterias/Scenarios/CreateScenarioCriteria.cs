namespace Score247.Management.Site.Criterias.Scenarios
{
    using System;
    using Fanex.Data.Repository;

    public class CreateScenarioCriteria : NonQueryCommand
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public string HomeId { get; set; }
        public string AwayId { get; set; }

        public DateTime EventDate { get; set; }

        public override string GetSettingKey()
            => "Scenario_Insert";

        public override bool IsValid()
            => true;
    }
}
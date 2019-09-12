namespace Score247Management.Core.Scenarios.Models
{
    using System;
    using System.Collections.Generic;

    public class Scenario
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public string HomeId { get; set; }
        public string AwayId { get; set; }
        public DateTime EventDate { get; set; }

        public IEnumerable<ScenarioEvent> ScenarioEvents { get; set; }
    }
}
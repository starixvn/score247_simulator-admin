namespace Score247Management.Core.Scenarios.Models
{
    public class ScenarioEvent
    {
        public int Id { get; set; }
        public int ScenarioId { get; set; }
        public string EventRequest { get; set; }
        public int Order { get; set; }
    }
}
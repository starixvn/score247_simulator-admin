namespace Score247Management.Core.Leagues.Models
{
    public class League
    {
        private string name;
        public string Id { get; set; }

        public string Name { get => name.Trim().TrimStart('*'); set => name = value; }

        public int Order { get; set; }

        public string Flag { get; set; }

        public LeagueCategory LeagueCategory { get; set; }

        public bool IsActive { get; set; }
    }
}
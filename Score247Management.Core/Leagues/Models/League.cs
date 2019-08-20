namespace Score247Management.Core.Leagues.Models
{
    using Score247Management.Core.Base;

    public class League : BaseModel
    {
        public int Order { get; set; }

        public string Flag { get; set; }

        public LeagueCategory LeagueCategory { get; set; }
    }
}
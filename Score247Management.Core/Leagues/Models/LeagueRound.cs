namespace Score247Management.Core.Leagues.Models
{
    using Score247Management.Core._Shared.Enumerations;

    public class LeagueRound
    {
        public string Id { get; set; }
        public LeagueRoundType Type { get; set; }

        public string Name { get; set; }

        public int Number { get; set; }

        public string Phase { get; set; }

        public string Group { get; set; }

        public int CupRoundMatchNumber { get; set; }
        public int CupRoundMatches { get; set; }
        public string OtherMatchId { get; set; }
    }
}
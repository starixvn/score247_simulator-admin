namespace Score247Management.Core.Odds.Models
{
    using System.Collections.Generic;

    public class MatchOddsMovement
    {
        public MatchOddsMovement()
        {
        }

        public MatchOddsMovement(
            string matchId,
            Bookmaker bookmaker,
            IEnumerable<OddsMovement> oddsMovements)
        {
            MatchId = matchId;
            Bookmaker = bookmaker;
            OddsMovements = oddsMovements;
        }

        public string MatchId { get; }

        public Bookmaker Bookmaker { get; }

        public IEnumerable<OddsMovement> OddsMovements { get; }
    }
}
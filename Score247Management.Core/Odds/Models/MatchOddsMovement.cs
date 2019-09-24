namespace Score247Management.Core.Odds.Models
{
    using System.Collections.Generic;
    using System.Linq;
    using MessagePack;

    [MessagePackObject(keyAsPropertyName: true)]
    public class MatchOddsMovement
    {
        public MatchOddsMovement()
        {
            OddsMovements = Enumerable.Empty<OddsMovement>();
        }

        [SerializationConstructor]
        public MatchOddsMovement(
            string matchId,
            Bookmaker bookmaker,
            IEnumerable<OddsMovement> oddsMovements)
        {
            MatchId = matchId;
            Bookmaker = bookmaker;
            OddsMovements = oddsMovements;
        }

        public string MatchId { get; set; }

        public Bookmaker Bookmaker { get; set; }

        public IEnumerable<OddsMovement> OddsMovements { get; set; }
    }
}
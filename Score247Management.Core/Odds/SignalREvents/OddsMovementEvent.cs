namespace Score247Management.Core.Odds.SignalREvents
{
    using Score247Management.Core.Odds.Models;

    public class OddsMovementEvent
    {
        public OddsMovementEvent(
            int betTypeId,
            Bookmaker bookmaker,
            OddsMovement oddsMovement)
        {
            BetTypeId = betTypeId;
            Bookmaker = bookmaker;
            OddsMovement = oddsMovement;
        }

        public int BetTypeId { get; }

        public Bookmaker Bookmaker { get; }

        public OddsMovement OddsMovement { get; }
    }
}
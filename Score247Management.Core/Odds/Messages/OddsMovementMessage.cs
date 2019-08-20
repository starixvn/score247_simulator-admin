namespace Score247Management.Core.Odds.Messages
{
    using System.Collections.Generic;
    using Score247Management.Core.Odds.SignalREvents;

    public interface IOddsMovementMessage
    {
        string MatchId { get; }

        IEnumerable<OddsMovementEvent> OddsEvents { get; }
    }

    public class OddsMovementMessage : IOddsMovementMessage
    {
        public OddsMovementMessage(
            string matchId,
            IEnumerable<OddsMovementEvent> oddsEvents)
        {
            MatchId = matchId;
            OddsEvents = oddsEvents;
        }

        public string MatchId { get; }

        public IEnumerable<OddsMovementEvent> OddsEvents { get; }
    }
}
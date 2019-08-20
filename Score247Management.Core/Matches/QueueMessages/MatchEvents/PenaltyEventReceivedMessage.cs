namespace Score247Management.Core.Matches.QueueMessages.MatchEvents
{
    using Score247Management.Core.Matches.Models;

    public interface IPenaltyEventReceivedMessage
    {
        MatchEvent MatchEvent { get; }
    }

    public class PenaltyEventReceivedMessage : IPenaltyEventReceivedMessage
    {
        public PenaltyEventReceivedMessage(MatchEvent matchEvent)
        {
            MatchEvent = matchEvent;
        }

        public MatchEvent MatchEvent { get; }
    }
}
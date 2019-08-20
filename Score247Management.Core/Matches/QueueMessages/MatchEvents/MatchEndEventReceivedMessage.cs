namespace Score247Management.Core.Matches.QueueMessages.MatchEvents
{
    using Score247Management.Core.Matches.Models;

    public interface IMatchEndEventReceivedMessage
    {
        MatchEvent MatchEvent { get; }
    }

    public class MatchEndEventReceivedMessage : IMatchEndEventReceivedMessage
    {
        public MatchEndEventReceivedMessage(MatchEvent matchEvent)
        {
            MatchEvent = matchEvent;
        }

        public MatchEvent MatchEvent { get; }
    }
}
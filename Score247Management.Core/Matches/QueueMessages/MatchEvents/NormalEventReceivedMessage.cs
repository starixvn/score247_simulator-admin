namespace Score247Management.Core.Matches.QueueMessages.MatchEvents
{
    using Score247Management.Core.Matches.Models;

    public interface INormalEventReceivedMessage
    {
        MatchEvent MatchEvent { get; }
    }

    public class NormalEventReceivedMessage : INormalEventReceivedMessage
    {
        public NormalEventReceivedMessage(MatchEvent matchEvent)
        {
            MatchEvent = matchEvent;
        }

        public MatchEvent MatchEvent { get; }
    }
}
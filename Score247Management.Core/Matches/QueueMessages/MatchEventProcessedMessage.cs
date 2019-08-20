namespace Score247Management.Core.Matches.QueueMessages
{
    using Score247Management.Core.Matches.Models;

    public interface IMatchEventProcessedMessage
    {
        MatchEvent MatchEvent { get; }
    }

    public class MatchEventProcessedMessage : IMatchEventProcessedMessage
    {
        public MatchEventProcessedMessage(MatchEvent matchEvent)
        {
            MatchEvent = matchEvent;
        }

        public MatchEvent MatchEvent { get; }
    }
}
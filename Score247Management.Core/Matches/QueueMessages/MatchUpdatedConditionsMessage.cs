namespace Score247Management.Core.Matches.QueueMessages
{
    using Score247Management.Core._Shared.Enumerations;

    public interface IMatchUpdatedConditionsMessage
    {
        string MatchId { get; }

        string Referee { get; }

        int Attendance { get; }

        Language Language { get; }
    }

    public class MatchUpdatedConditionsMessage : IMatchUpdatedConditionsMessage
    {
        public MatchUpdatedConditionsMessage(string matchId, string referee, int attendance, Language language)
        {
            MatchId = matchId;
            Referee = referee;
            Attendance = attendance;
            Language = language;
        }

        public string MatchId { get; }

        public string Referee { get; }

        public int Attendance { get; }

        public Language Language { get; }
    }
}

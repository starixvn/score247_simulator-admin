namespace Score247Management.Core.Matches.Events
{
    using System.Collections.Generic;
    using Score247Management.Core.Matches.Models;

    public interface IPreMatchesFetchedMessage
    {
        IEnumerable<Match> Matches { get; }

        string Language { get; }
    }

    public class PreMatchesFetchedMessage : IPreMatchesFetchedMessage
    {
        public PreMatchesFetchedMessage(IEnumerable<Match> matches, string language)
        {
            Matches = matches;
            Language = language;
        }

        public IEnumerable<Match> Matches { get; }

        public string Language { get; }
    }
}
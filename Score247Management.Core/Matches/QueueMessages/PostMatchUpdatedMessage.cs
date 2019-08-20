namespace Score247Management.Core.Matches.Events
{
    using System.Collections.Generic;
    using Score247Management.Core.Matches.Models;

    public interface IPostMatchFetchedMessage
    {
        IEnumerable<Match> Matches { get; }

        string Language { get; }
    }

    public class PostMatchFetchedMessage : IPostMatchFetchedMessage
    {
        public PostMatchFetchedMessage(IEnumerable<Match> matches, string language)
        {
            Matches = matches;
            Language = language;
        }

        public IEnumerable<Match> Matches { get; }

        public string Language { get; }
    }
}
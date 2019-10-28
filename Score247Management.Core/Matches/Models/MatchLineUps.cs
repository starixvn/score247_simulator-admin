using System;
using MessagePack;
using Newtonsoft.Json;
using Score247Management.Core.Teams.Models;

namespace Score247Management.Core.Matches.Models
{
    [MessagePackObject(keyAsPropertyName: true)]
    public class MatchLineups
    {
        public MatchLineups()
        {
        }

        [SerializationConstructor, JsonConstructor]
        public MatchLineups(string id, DateTimeOffset eventDate, TeamLineups home, TeamLineups away)
        {
            Id = id;
            EventDate = eventDate;
            Home = home;
            Away = away;
        }

        public string Id { get; }

        public DateTimeOffset EventDate { get; }

        public TeamLineups Home { get; }

        public TeamLineups Away { get; }
    }
}
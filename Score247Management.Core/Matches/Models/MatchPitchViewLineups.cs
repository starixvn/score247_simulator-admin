using System;
using MessagePack;
using Score247Management.Core.Teams.Models;

namespace Score247Management.Core.Matches.Models
{
    [MessagePackObject(keyAsPropertyName: true)]
    public class MatchPitchViewLineups
    {
        public string Id { get; set; }

        public DateTimeOffset EventDate { get; set; }

        public TeamLineups Home { get; set; }

        public TeamLineups Away { get; set; }

        public string PitchView { get; set; }
    }
}
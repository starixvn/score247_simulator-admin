using System;
using MessagePack;
using Score247Management.Core.Teams.Models;

namespace Score247Management.Core.Matches.Models
{
    [MessagePackObject]
    public class MatchPitchViewLineups
    {
        [Key(0)]
        public string Id { get; set; }

        [Key(1)]
        public DateTimeOffset EventDate { get; set; }

        [Key(2)]
        public TeamLineups Home { get; set; }

        [Key(3)]
        public TeamLineups Away { get; set; }

        [Key(4)]
        public string PitchView { get; set; }
    }
}
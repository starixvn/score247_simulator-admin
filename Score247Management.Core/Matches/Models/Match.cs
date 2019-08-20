namespace Score247Management.Core.Matches.Models
{
    using System;
    using System.Collections.Generic;
    using Newtonsoft.Json;
    using Score247Management.Core.Base;
    using Score247Management.Core.Leagues.Models;
    using Score247Management.Core.Teams.Models;

    public sealed class Match : BaseEntity
    {
        public DateTimeOffset EventDate { get; set; }

        public IEnumerable<Team> Teams { get; set; }

        public MatchResult MatchResult { get; set; }

        public League League { get; set; }

        public LeagueRound LeagueRound { get; set; }

        public IEnumerable<TimelineEvent> TimeLines { get; set; }

        public TimelineEvent LatestTimeline { get; set; }

        public int Attendance { get; set; }

        [JsonIgnore]
        public string Value { get; set; }

        public Venue Venue { get; set; }

        public string Referee { get; set; }

        public string Region { get; set; }
    }
}
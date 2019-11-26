using Score247Management.Core.Matches.Models.Soccer.Core.Matches.Models;

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
        public DateTime EventDate { get; set; }

        public string DisplayDate => EventDate.ToUniversalTime().ToString("dd/MM/yyyy HH:mm:ss");

        public DateTimeOffset CurrentPeriodStartTime { get; set; }
        public IEnumerable<Team> Teams { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public MatchResult MatchResult { get; set; }

        public League League { get; set; }

        public LeagueRound LeagueRound { get; set; }
        public LeagueSeason LeagueSeason { get; set; }

        public IEnumerable<TimelineEvent> TimeLines { get; set; }

        public TimelineEvent LatestTimeline { get; set; }

        public int Attendance { get; set; }

        [JsonIgnore]
        public string Value { get; set; }

        public Venue Venue { get; set; }

        public string Referee { get; set; }

        public string Region { get; set; }

        public Coverage Coverage { get; set; }

        public string DisplayResult
        {
            get => MatchResult.EventStatus.IsClosed() ?
                    MatchResult.HomeScore.ToString() + " - " + MatchResult.AwayScore.ToString() :
                    LatestTimeline != null ?
                        LatestTimeline.HomeScore.ToString() + " - " + LatestTimeline.AwayScore.ToString() :
                        "";
        }
    }
}
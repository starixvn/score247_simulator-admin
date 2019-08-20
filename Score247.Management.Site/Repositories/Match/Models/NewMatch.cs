using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Score247Management.Core.Leagues.Models;
using Score247Management.Core.Matches.Models;

namespace Score247.Management.Site.Repositories.Match.Models
{
    public class NewMatch
    {
        public string Id { get; set; }
        public string Status { get; set; }

        [DataType(DataType.Date)]
        public DateTimeOffset Scheduled { get; set; }

        [DataType(DataType.Time)]
        public TimeSpan ScheduledTime { get; set; }
        public LeagueRound TournamentRound { get; set; }
        public LeagueSeason Season { get; set; }
        public Score247Management.Core.Leagues.Models.League Tournament { get; set; }
        public IEnumerable<Score247Management.Core.Teams.Models.Team> Competitors { get; set; }
        public Venue Venue { get; set; }

    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Score247.Management.Site.Helpers;
using Score247Management.Core.Leagues.Models;
using Score247Management.Core.Matches.Models;
using Score247Management.Core.Matches.Models.Soccer.Core.Matches.Models;
using Score247Management.Core.Teams.Models;

namespace Score247.Management.Site.Repositories.Matches.Models
{
    public class MatchEntity
    {
        public string Id { get; set; }
        public string Status { get; set; }
        public string MatchStatus { get; set; }

        [DataType(DataType.Date)]
        public DateTime Scheduled { get; set; }

        [DataType(DataType.Time)]
        public TimeSpan ScheduledTime { get; set; }

        public LeagueRound TournamentRound { get; set; }
        public LeagueSeason Season { get; set; }
        public Score247Management.Core.Leagues.Models.League Tournament { get; set; }
        public IEnumerable<Team> Competitors { get; set; }
        public Venue Venue { get; set; }
        public string Referee { get; set; }
        public Coverage Coverage { get; set; }

        public MatchEntity()
        {
            Scheduled = DateTime.Now;
            ScheduledTime = new TimeSpan(Scheduled.Hour, 0, 0);
            Venue = new Venue()
            {
                Name = "Default Venue"
            };
            TournamentRound = new LeagueRound()
            {
                Name = "Round",
                Number = 2,
                Phase = "Round",
                Group = "A"
            };
            Coverage = new Coverage(true, "", true, true, true, true, true);
        }

        public MatchEntity(League league, Team homeTeam, Team awayTeam, DateTime eventDate)
        {
            Scheduled = eventDate;
            ScheduledTime = new TimeSpan(Scheduled.Hour, 0, 0);
            Venue = new Venue()
            {
                Name = "Default Venue"
            };
            TournamentRound = new LeagueRound()
            {
                Name = "Round",
                Number = 2,
                Phase = "Round",
                Group = "A"
            };
            Tournament = league;
            Id = UniqueGenerator.GetIdWithPattern("sr:match:");
            homeTeam.Statistic = new TeamStatistic();
            homeTeam.qualifier = "home";
            awayTeam.Statistic = new TeamStatistic();
            awayTeam.qualifier = "away";
            Competitors = new List<Team>() { homeTeam, awayTeam };
            Coverage = new Coverage(true, "", true, true, true, true, true);
        }
    }
}
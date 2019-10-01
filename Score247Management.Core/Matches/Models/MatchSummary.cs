namespace Score247Management.Core.Matches.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using MessagePack;
    using Score247Management.Core._Shared.Enumerations;

    [MessagePackObject]
#pragma warning disable S109 // Magic numbers should not be used
    public class MatchSummary
    {
#pragma warning disable S107 // Methods should not have too many parameters

        [SerializationConstructor]
        public MatchSummary(
            string id,
            DateTimeOffset eventDate,
            DateTimeOffset currentPeriodStartTime,
            string leagueId,
            string leagueName,
            string homeTeamId,
            string homeTeamName,
            string awayTeamId,
            string awayTeamName,
            MatchStatus matchStatus,
            MatchStatus eventStatus,
            byte homeScore,
            byte awayScore,
            string winnerId,
            string aggregateWinnerId,
            byte aggregateHomeScore,
            byte aggregateAwayScore,
            byte homeRedCards,
            byte homeYellowRedCards,
            byte awayRedCards,
            byte awayYellowRedCards,
            byte matchTime,
            string stoppageTime,
            byte injuryTimeAnnounced,
            EventType lastTimelineType,
            IEnumerable<MatchPeriod> matchPeriods,
            string countryCode,
            string countryName,
            DateTimeOffset modifiedTime)
#pragma warning restore S107 // Methods should not have too many parameters
        {
            Id = id;
            EventDate = eventDate;
            CurrentPeriodStartTime = currentPeriodStartTime;
            LeagueId = leagueId;
            LeagueName = leagueName;
            HomeTeamId = homeTeamId;
            HomeTeamName = homeTeamName;
            AwayTeamId = awayTeamId;
            AwayTeamName = awayTeamName;
            MatchStatus = matchStatus;
            EventStatus = eventStatus;
            HomeScore = homeScore;
            AwayScore = awayScore;
            WinnerId = winnerId;
            AggregateWinnerId = aggregateWinnerId;
            AggregateHomeScore = aggregateHomeScore;
            AggregateAwayScore = aggregateAwayScore;
            HomeRedCards = homeRedCards;
            HomeYellowRedCards = homeYellowRedCards;
            AwayRedCards = awayRedCards;
            AwayYellowRedCards = awayYellowRedCards;
            MatchTime = matchTime;
            StoppageTime = stoppageTime;
            InjuryTimeAnnounced = injuryTimeAnnounced;
            LastTimelineType = lastTimelineType;
            MatchPeriods = matchPeriods;
            CountryCode = countryCode;
            CountryName = countryName;
            ModifiedTime = modifiedTime;
        }

        public MatchSummary()
        {
        }

        public MatchSummary(Match match)
        {
            Id = match.Id;
            EventDate = match.EventDate;
            CurrentPeriodStartTime = match.CurrentPeriodStartTime;
            ModifiedTime = match.ModifiedTime;

            if (match.League != null)
            {
                LeagueId = match.League.Id;
                LeagueName = match.League.Name;
                CountryCode = match.League.LeagueCategory?.CountryCode;
                CountryName = match.League.LeagueCategory?.Name;
            }

            AssignMatchResult(match);
            AssignTeamInformation(match);
            AssignLatestTimeline(match);
        }

        private void AssignLatestTimeline(Match match)
        {
            if (match.LatestTimeline != null)
            {
                StoppageTime = match.LatestTimeline.StoppageTime;
                InjuryTimeAnnounced = (byte)match.LatestTimeline.InjuryTimeAnnounced;
                LastTimelineType = match.LatestTimeline.Type;
            }
        }

        private void AssignMatchResult(Match match)
        {
            if (match.MatchResult != null)
            {
                EventStatus = match.MatchResult.EventStatus;
                MatchStatus = match.MatchResult.MatchStatus;
                HomeScore = (byte)match.MatchResult.HomeScore;
                AwayScore = (byte)match.MatchResult.AwayScore;
                WinnerId = match.MatchResult.WinnerId;
                AggregateWinnerId = match.MatchResult.AggregateWinnerId;
                AggregateHomeScore = (byte)match.MatchResult.AggregateHomeScore;
                AggregateAwayScore = (byte)match.MatchResult.AggregateAwayScore;
                MatchTime = (byte)match.MatchResult.MatchTime;
                MatchPeriods = match.MatchResult.MatchPeriods;
            }
        }

        private void AssignTeamInformation(Match match)
        {
            const int twoTeams = 2;

            if (match.Teams != null && match.Teams.Count() >= twoTeams)
            {
                var homeTeam = match.Teams.FirstOrDefault(t => t.IsHome);
                var awayTeam = match.Teams.FirstOrDefault(t => !t.IsHome);

                if (homeTeam != null)
                {
                    HomeTeamId = homeTeam.Id;
                    HomeTeamName = homeTeam.Name;

                    if (homeTeam.Statistic != null)
                    {
                        HomeRedCards = (byte)homeTeam.Statistic.RedCards;
                        HomeYellowRedCards = (byte)homeTeam.Statistic.YellowRedCards;
                    }
                }

                if (awayTeam != null)
                {
                    AwayTeamId = awayTeam.Id;
                    AwayTeamName = awayTeam.Name;

                    if (awayTeam.Statistic != null)
                    {
                        AwayRedCards = (byte)awayTeam.Statistic.RedCards;
                        AwayYellowRedCards = (byte)awayTeam.Statistic.YellowRedCards;
                    }
                }
            }
        }

        [Key(0)]
        public string Id { get; private set; }

        [Key(1)]
        public DateTimeOffset EventDate { get; private set; }

        [IgnoreMember]
        public string DisplayDate { get; set; }

        [Key(2)]
        public DateTimeOffset CurrentPeriodStartTime { get; private set; }

        [Key(3)]
        public string LeagueId { get; private set; }

        [Key(4)]
        public string LeagueName { get; private set; }

        [Key(5)]
        public string HomeTeamId { get; private set; }

        [Key(6)]
        public string HomeTeamName { get; private set; }

        [Key(7)]
        public string AwayTeamId { get; private set; }

        [Key(8)]
        public string AwayTeamName { get; private set; }

        [Key(9)]
        public MatchStatus MatchStatus { get; private set; }

        [Key(10)]
        public MatchStatus EventStatus { get; private set; }

        [Key(11)]
        public byte HomeScore { get; private set; }

        [Key(12)]
        public byte AwayScore { get; private set; }

        [Key(13)]
        public string WinnerId { get; private set; }

        [Key(14)]
        public string AggregateWinnerId { get; private set; }

        [Key(15)]
        public byte AggregateHomeScore { get; private set; }

        [Key(16)]
        public byte AggregateAwayScore { get; private set; }

        [Key(17)]
        public byte HomeRedCards { get; private set; }

        [Key(18)]
        public byte HomeYellowRedCards { get; private set; }

        [Key(19)]
        public byte AwayRedCards { get; private set; }

        [Key(20)]
        public byte AwayYellowRedCards { get; private set; }

        [Key(21)]
        public byte MatchTime { get; private set; }

        [Key(22)]
        public string StoppageTime { get; private set; }

        [Key(23)]
        public byte InjuryTimeAnnounced { get; private set; }

        [Key(24)]
        public EventType LastTimelineType { get; private set; }

        [Key(25)]
        public IEnumerable<MatchPeriod> MatchPeriods { get; private set; }

        [Key(26)]
        public string CountryCode { get; private set; }

        [Key(27)]
        public string CountryName { get; private set; }

        [Key(28)]
        public DateTimeOffset ModifiedTime { get; private set; }

        [IgnoreMember]
        public string DisplayResult
        {
            get => HomeScore.ToString() + " - " + AwayScore.ToString();
        }
    }
}
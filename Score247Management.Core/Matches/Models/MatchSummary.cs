namespace Score247Management.Core.Matches.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using MessagePack;
    using Newtonsoft.Json;
    using Score247Management.Core._Shared.Enumerations;

    [MessagePackObject(keyAsPropertyName: true)]
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
            DateTimeOffset modifiedTime,
            bool isInternationalLeague,
            int leagueOrder,
            string leagueSeasonId,
            LeagueRoundType leagueRoundType,
            string leagueRoundName,
            int leagueRoundNumber,
            string leagueRoundGroup,
            string leagueGroupName)
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
            IsInternationalLeague = isInternationalLeague;
            LeagueOrder = leagueOrder;
            LeagueSeasonId = leagueSeasonId;
            LeagueRoundType = leagueRoundType;
            LeagueRoundName = leagueRoundName;
            LeagueRoundNumber = leagueRoundNumber;
            LeagueRoundGroup = leagueRoundGroup;
            LeagueGroupName = leagueGroupName;
        }

        public MatchSummary()
        {
        }

        [JsonProperty("id")]
        public string Id { get; private set; }

        [JsonProperty("eventDate")]
        public DateTimeOffset EventDate { get; private set; }

        [JsonProperty("currentPeriodStartTime")]
        public DateTimeOffset CurrentPeriodStartTime { get; private set; }

        [JsonProperty("leagueId")]
        public string LeagueId { get; private set; }

        [JsonProperty("leagueName")]
        public string LeagueName { get; private set; }

        [JsonProperty("homeTeamId")]
        public string HomeTeamId { get; private set; }

        [JsonProperty("homeTeamName")]
        public string HomeTeamName { get; private set; }

        [JsonProperty("awayTeamId")]
        public string AwayTeamId { get; private set; }

        [JsonProperty("awayTeamName")]
        public string AwayTeamName { get; private set; }

        [JsonProperty("matchStatus")]
        public MatchStatus MatchStatus { get; private set; }

        [JsonProperty("eventStatus")]
        public MatchStatus EventStatus { get; private set; }

        [JsonProperty("homeScore")]
        public byte HomeScore { get; private set; }

        [JsonProperty("awayScore")]
        public byte AwayScore { get; private set; }

        [JsonProperty("ưinnerId")]
        public string WinnerId { get; private set; }

        [JsonProperty("aggregateWinnerId")]
        public string AggregateWinnerId { get; private set; }

        [JsonProperty("aggregateHomeScore")]
        public byte AggregateHomeScore { get; private set; }

        [JsonProperty("aggregateAwayScore")]
        public byte AggregateAwayScore { get; private set; }

        [JsonProperty("homeRedCards")]
        public byte HomeRedCards { get; private set; }

        [JsonProperty("homeYellowRedCards")]
        public byte HomeYellowRedCards { get; private set; }

        [JsonProperty("awayRedCards")]
        public byte AwayRedCards { get; private set; }

        [JsonProperty("awayYellowRedCards")]
        public byte AwayYellowRedCards { get; private set; }

        [JsonProperty("matchTime")]
        public byte MatchTime { get; private set; }

        [JsonProperty("stoppageTime")]
        public string StoppageTime { get; private set; }

        [JsonProperty("injuryTimeAnnounced")]
        public byte InjuryTimeAnnounced { get; private set; }

        [JsonProperty("lastTimelineType")]
        public EventType LastTimelineType { get; private set; }

        [JsonProperty("matchPeriods")]
        public IEnumerable<MatchPeriod> MatchPeriods { get; private set; }

        [JsonProperty("countryCode")]
        public string CountryCode { get; private set; }

        [JsonProperty("countryName")]
        public string CountryName { get; private set; }

        [JsonProperty("modifiedTime")]
        public DateTimeOffset ModifiedTime { get; private set; }

        [JsonProperty("isInternationalLeague")]
        public bool IsInternationalLeague { get; private set; }

        [JsonProperty("leagueOrder")]
        public int LeagueOrder { get; private set; }

        [JsonProperty("leagueSeasonId")]
        public string LeagueSeasonId { get; private set; }

        [JsonProperty("leagueRoundType")]
        public LeagueRoundType LeagueRoundType { get; private set; }

        [JsonProperty("leagueRoundName")]
        public string LeagueRoundName { get; private set; }

        [JsonProperty("leagueRoundNumber")]
        public int LeagueRoundNumber { get; private set; }

        [JsonProperty("leagueRoundGroup")]
        public string LeagueRoundGroup { get; private set; }

        [JsonProperty("leagueGroupName")]
        public string LeagueGroupName { get; private set; }

        [IgnoreMember]
        public string DisplayResult
        {
            get => HomeScore.ToString() + " - " + AwayScore.ToString();
        }

        [IgnoreMember]
        public string DisplayDate { get; set; }
    }
}
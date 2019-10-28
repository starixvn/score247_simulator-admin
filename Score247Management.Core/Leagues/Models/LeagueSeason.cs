namespace Score247Management.Core.Leagues.Models
{
    using System;
    using Newtonsoft.Json;

    public class LeagueSeason
    {
        public string Id { get; set; }
        public string Name { get; set; }

        [JsonProperty("start_date")]
        public DateTimeOffset StartDate { get; set; }

        [JsonProperty("end_date")]
        public DateTimeOffset EndDate { get; set; }

        public string Year { get; set; }

        [JsonProperty("tournament_id")]
        public string TournamentId { get; set; }
    }
}
using System;
using Newtonsoft.Json;
using Score247Management.Core.Base;

namespace Score247Management.Core.Leagues.Models
{
    public class LeagueSeason : BaseModel
    {
        [JsonProperty("start_date")]
        public DateTimeOffset StartDate { get; set; }

        [JsonProperty("end_date")]
        public DateTimeOffset EndDate { get; set; }

        public string Year { get; set; }

        [JsonProperty("tournament_id")]
        public string TournamentId { get; set; }
    }
}
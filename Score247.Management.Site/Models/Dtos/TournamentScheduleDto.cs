using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Score247.Management.Site.Models.Events;

namespace Score247.Management.Site.Models.Dtos
{
    public class TournamentScheduleDto
    {
        public DateTime generated_at { get; set; }
        public string schema { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string message { get; set; }

        public Tournament tournament { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public IEnumerable<Sport_Event> sport_events { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public IEnumerable<Result> results { get; set; }
    }
}
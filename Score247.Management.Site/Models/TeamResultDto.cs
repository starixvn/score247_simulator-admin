namespace Score247.Management.Site.Models
{
    using System;
    using System.Collections.Generic;
    using Newtonsoft.Json;
    using Score247.Management.Site.Models.Events;

    public class TeamResultDto
    {
        public DateTime generated_at { get; set; }
        public string schema { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public Competitor team { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public IEnumerable<Result> results { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string message { get; set; }
    }
}
namespace Score247.Management.Site.Models
{
    using System;
    using System.Collections.Generic;
    using Newtonsoft.Json;
    using Score247.Management.Site.Models.Events;

    public class H2HDto
    {
        public DateTime generated_at { get; set; }
        public string schema { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public IEnumerable<Competitor> teams { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public LastMeetings last_meetings { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public IEnumerable<Result> next_meetings { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string message { get; set; }
    }

    public class LastMeetings
    {
        public IEnumerable<Result> results { get; set; }
    }
}
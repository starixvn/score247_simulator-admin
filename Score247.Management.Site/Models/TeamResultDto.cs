namespace Score247.Management.Site.Models
{
    using System;
    using System.Collections.Generic;
    using Score247.Management.Site.Models.Events;

    public class TeamResultDto
    {
        public DateTime generated_at { get; set; }
        public string schema { get; set; }
        public Competitor team { get; set; }
        public IEnumerable<Result> results { get; set; }
    }
}
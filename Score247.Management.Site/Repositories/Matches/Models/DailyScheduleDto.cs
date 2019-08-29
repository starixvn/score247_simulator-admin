using System;
using System.Collections.Generic;

namespace Score247.Management.Site.Repositories.Matches.Models
{
    public class DailyScheduleDto
    {
        public DateTime generated_at { get; set; }
        public string schema { get; set; }
        public IEnumerable<MatchEntity> sport_events { get; set; }
    }
}
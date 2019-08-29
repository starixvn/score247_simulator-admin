using System;
using System.Collections.Generic;

namespace Score247.Management.Site.Repositories.Matches.Models
{
    public class DailyScheduleDto
    {
        public DateTime GeneratedAt { get; set; }
        public string Schema { get; set; }
        public IEnumerable<MatchEntity> sport_events { get; set; }
    }
}
using System;
using System.Collections.Generic;

namespace Score247Management.Repositories.Match.Models
{
    public class DailyScheduleDto
    {
        public DateTime GeneratedAt { get; set; }
        public string Schema { get; set; }
        public IEnumerable<NewMatch> sport_events { get; set; }
    }
}

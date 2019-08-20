using System;
using Score247Management.Core.Base;

namespace Score247Management.Core.Leagues.Models
{
    public class LeagueSeason : BaseModel
    {
        public DateTimeOffset StartDate { get; set; }
        public DateTimeOffset EndDate { get; set; }
        public string Year { get; set; }
        public string TournamentId { get; set; }
    }
}

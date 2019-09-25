using System;
using System.Collections.Generic;
using Score247Management.Core.Leagues.Models;

namespace Score247Management.Core.Leagues.Dtos
{
    public class LeagueSportradarList
    {
        public DateTimeOffset generated_at { get; set; }
        public string schema { get; set; }
        public IEnumerable<League> tournaments { get; set; }
    }
}
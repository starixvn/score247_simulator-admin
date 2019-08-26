using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Score247Management.Core.Leagues.Models;

namespace Score247.Management.Site.Models
{
    public class DateSearchModel
    {
        [DataType(DataType.Date)]
        public DateTime FromDate {get;set; }

        [DataType(DataType.Date)]
        public DateTime ToDate { get; set; }

        public IEnumerable<League> Leagues { get; set; }

        public string SelectedLeagueId { get; set; }
    }
}

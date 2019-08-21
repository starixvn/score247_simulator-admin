using System;
using System.ComponentModel.DataAnnotations;

namespace Score247.Management.Site.Models
{
    public class DateSearchModel
    {
        [DataType(DataType.Date)]
        public DateTime FromDate {get;set; }

        [DataType(DataType.Date)]
        public DateTime ToDate { get; set; }
    }
}

namespace Score247.Management.Site.Models
{
    using System;
    using System.ComponentModel.DataAnnotations;

    public class CloneMatchModel
    {
        public int Count { get; set; }

        [DataType(DataType.Date)]
        public DateTime EventDate { get; set; }
    }
}

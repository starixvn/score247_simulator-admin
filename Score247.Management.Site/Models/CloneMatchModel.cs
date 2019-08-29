namespace Score247.Management.Site.Models
{
    using System;
    using System.ComponentModel.DataAnnotations;

    public class CloneMatchModel
    {
        public int Count { get; set; }

        [DataType(DataType.Date)]

        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime EventDate { get; set; }
    }
}
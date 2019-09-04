namespace Score247.Management.Site.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    public class CreateDeleteMatchesModel
    {
        [DataType(DataType.Date)]
        public DateTime EventDate { get; set; }

        public int Count { get; set; }

        public string DisplayDate { get; set; }

        public static List<CreateDeleteMatchesModel> CreateFromToDay()
        {
            var result = new List<CreateDeleteMatchesModel>();
            var today = DateTime.Now;
            result.Add(new CreateDeleteMatchesModel()
            {
                Count = 0,
                EventDate = today.AddDays(-3),
                DisplayDate = today.AddDays(-3).ToString("ddd, dd MMM")
            });
            result.Add(new CreateDeleteMatchesModel()
            {
                Count = 0,
                EventDate = today.AddDays(-2),
                DisplayDate = today.AddDays(-2).ToString("ddd, dd MMM")
            });
            result.Add(new CreateDeleteMatchesModel()
            {
                Count = 0,
                EventDate = today.AddDays(-1),
                DisplayDate = today.AddDays(-1).ToString("ddd, dd MMM")
            });
            result.Add(new CreateDeleteMatchesModel()
            {
                Count = 0,
                EventDate = today,
                DisplayDate = today.ToString("'Today', dd MMM")
            });
            result.Add(new CreateDeleteMatchesModel()
            {
                Count = 0,
                EventDate = today.AddDays(1),
                DisplayDate = today.AddDays(1).ToString("ddd, dd MMM")
            });
            result.Add(new CreateDeleteMatchesModel()
            {
                Count = 0,
                EventDate = today.AddDays(2),
                DisplayDate = today.AddDays(2).ToString("ddd, dd MMM")
            });
            return result;
        }
    }
}
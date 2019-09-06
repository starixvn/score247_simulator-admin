namespace Score247Management.Core.Teams.Models
{
    using System.Collections.Generic;
    using Score247Management.Core.Base;

    public class Team : BaseModel
    {
        public string Country { get; set; }

        public string CountryCode { get; set; }

        public string Flag { get; set; }

        public bool IsHome { get; set; }

        public IEnumerable<Player> Players { get; set; }

        public TeamStatistics Statistic { get; set; }

        public Coach Coach { get; set; }

        public string Formation { get; set; }

        public string Abbreviation { get; set; }

        public IEnumerable<Player> Substitutions { get; set; }
        public string qualifier { get; set; }
    }
}
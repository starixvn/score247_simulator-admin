namespace Score247Management.Core.Matches.Models
{
    using Score247Management.Core.Base;

    public class Venue : BaseModel
    {
        public int Capacity { get; set; }

        public string CityName { get; set; }

        public string CountryName { get; set; }

        public string CountryCode { get; set; }
    }
}
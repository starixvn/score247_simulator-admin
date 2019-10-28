namespace Score247Management.Core.Matches.Models
{
    public class Venue
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public int Capacity { get; set; }

        public string CityName { get; set; }

        public string CountryName { get; set; }

        public string CountryCode { get; set; }
    }
}
namespace Score247Management.Core.Teams.Models
{
    using Score247Management.Core.Base;

    public class Coach : BaseModel
    {
        public string Nationality { get; set; }

        public string CountryCode { get; set; }
    }
}
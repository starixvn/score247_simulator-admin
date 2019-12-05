namespace Score247Management.Core.Teams.Models
{
    using MessagePack;
    using Score247Management.Core.Base;

    [MessagePackObject(keyAsPropertyName: true)]
    public class Coach : BaseModel
    {
        public string Nationality { get; set; }

        public string CountryCode { get; set; }
    }
}
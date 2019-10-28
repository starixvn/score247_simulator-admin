namespace Score247Management.Core.Teams.Models
{
    using MessagePack;
    using Score247Management.Core.Base;

    [MessagePackObject]
    public class Coach : BaseModel
    {
        [Key(2)]
        public string Nationality { get; set; }

        [Key(3)]
        public string CountryCode { get; set; }
    }
}
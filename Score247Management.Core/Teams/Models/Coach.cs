namespace Score247Management.Core.Teams.Models
{
    using MessagePack;
    using Newtonsoft.Json;
    using Score247Management.Core.Base;

    [MessagePackObject(keyAsPropertyName: true)]
    public class Coach : BaseModel
    {
        [SerializationConstructor, JsonConstructor]
        protected Coach(string id, string name) : base(id, name)
        {
        }

        public string Nationality { get; set; }

        public string CountryCode { get; set; }
    }
}
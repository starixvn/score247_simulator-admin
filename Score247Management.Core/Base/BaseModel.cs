namespace Score247Management.Core.Base
{
    using MessagePack;
    using Newtonsoft.Json;

    [MessagePackObject]
    public class BaseModel
    {
        [SerializationConstructor, JsonConstructor]
        protected BaseModel(string id, string name)
        {
            Id = id;
            Name = name;
        }

        [Key(0)]
        public string Id { get; set; }

        [Key(1)]
        public string Name { get; set; }
    }
}
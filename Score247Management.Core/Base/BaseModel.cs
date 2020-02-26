namespace Score247Management.Core.Base
{
    using MessagePack;
    using Newtonsoft.Json;

    [MessagePackObject(keyAsPropertyName: true)]
    public class BaseModel
    {
        [SerializationConstructor, JsonConstructor]
        protected BaseModel(string id, string name)
        {
            Id = id;
            Name = name;
        }

        public string Id { get; set; }

        public string Name { get; set; }
    }
}
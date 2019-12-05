namespace Score247Management.Core.Base
{
    using MessagePack;

    [MessagePackObject(keyAsPropertyName: true)]
    public class BaseModel
    {
        public string Id { get; set; }

        public string Name { get; set; }
    }
}
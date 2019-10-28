namespace Score247Management.Core.Base
{
    using MessagePack;

    [MessagePackObject]
    public class BaseModel
    {
        [Key(0)]
        public string Id { get; set; }

        [Key(1)]
        public string Name { get; set; }
    }
}
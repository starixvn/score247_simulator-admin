namespace Score247Management.Core.Teams.Models
{
    using MessagePack;
    using Newtonsoft.Json;
    using Score247Management.Core.Base;

    [MessagePackObject]
    public class Player : BaseModel
    {
        public Player(string id, string name) : base(id, name)
        {
        }

        [SerializationConstructor, JsonConstructor]
        public Player(string id, string name, PlayerType type, int jerseyNumber, Position position, int order) : base(id, name)
        {
            Type = type;
            JerseyNumber = jerseyNumber;
            Position = position;
            Order = order;
        }

        [Key(2)]
        public PlayerType Type { get; }

        [Key(3)]
        public int JerseyNumber { get; }

        [Key(4)]
        public Position Position { get; }

        [Key(5)]
        public int Order { get; }
    }

    [MessagePackObject]
    public class GoalScorer : BaseModel
    {
        [SerializationConstructor, JsonConstructor]
        public GoalScorer(string id, string name, string method) : base(id, name)
        {
            Method = method;
        }

        [Key(2)]
        public string Method { get; }
    }
}
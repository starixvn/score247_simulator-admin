namespace Score247Management.Core.Teams.Models
{
    using MessagePack;
    using Newtonsoft.Json;
    using Score247Management.Core.Base;

    [MessagePackObject(keyAsPropertyName: true)]
    public class Player : BaseModel
    {
        public PlayerType Type { get; set; }

        public int JerseyNumber { get; set; }

        public Position Position { get; set; }

        public int Order { get; set; }
    }

    [MessagePackObject(keyAsPropertyName: true)]
    public class GoalScorer : BaseModel
    {
        public string Method { get; set; }
    }
}
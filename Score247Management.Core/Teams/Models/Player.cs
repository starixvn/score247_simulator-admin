namespace Score247Management.Core.Teams.Models
{
    using MessagePack;
    using Score247Management.Core.Base;

    [MessagePackObject]
    public class Player : BaseModel
    {
        [Key(2)]
        public PlayerType Type { get; set; }

        [Key(3)]
        public int JerseyNumber { get; set; }

        [Key(4)]
        public Position Position { get; set; }

        [Key(5)]
        public int Order { get; set; }
    }

    [MessagePackObject]
    public class GoalScorer : BaseModel
    {
        [Key(2)]
        public string Method { get; set; }
    }
}
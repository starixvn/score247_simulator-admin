namespace Score247Management.Core.Teams.Models
{
    using Score247Management.Core.Base;

    public class Player : BaseModel
    {
        public string Type { get; set; }

        public int JerseyNumber { get; set; }

        public string Position { get; set; }

        public int Order { get; set; }
    }

    public class GoalScorer : BaseModel
    {
        public string Method { get; set; }
    }
}
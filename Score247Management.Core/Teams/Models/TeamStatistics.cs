namespace Score247Management.Core.Teams.Models
{
    using MessagePack;

    [MessagePackObject]
    public class TeamStatistic
    {
        public TeamStatistic()
        {
        }

        [Key(0)]
        public int Possession { get; set; }

        [Key(1)]
        public int FreeKicks { get; set; }

        [Key(2)]
        public int ThrowIns { get; set; }

        [Key(3)]
        public int GoalKicks { get; set; }

        [Key(4)]
        public int ShotsBlocked { get; set; }

        [Key(5)]
        public int ShotsOnTarget { get; set; }

        [Key(6)]
        public int ShotsOffTarget { get; set; }

        [Key(7)]
        public int CornerKicks { get; set; }

        [Key(8)]
        public int Fouls { get; set; }

        [Key(9)]
        public int ShotsSaved { get; set; }

        [Key(10)]
        public int Offsides { get; set; }

        [Key(11)]
        public int YellowCards { get; set; }

        [Key(12)]
        public int Injuries { get; set; }

        [Key(13)]
        public int RedCards { get; set; }

        [Key(14)]
        public int YellowRedCards { get; set; }
    }
}
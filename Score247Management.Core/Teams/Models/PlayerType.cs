namespace Score247Management.Core.Teams.Models
{
    using MessagePack;
    using Score247Management.Core.Enumerations;

    [MessagePackObject]
    public class PlayerType : Enumeration
    {
        public static readonly PlayerType Goalkeeper = new PlayerType(1, "goalkeeper");
        public static readonly PlayerType Defender = new PlayerType(2, "defender");
        public static readonly PlayerType Midfielder = new PlayerType(3, "midfielder");
        public static readonly PlayerType Forward = new PlayerType(4, "forward");
        public static readonly PlayerType Unknown = new PlayerType(5, "Unknown");

        public PlayerType()
        {
        }

        public PlayerType(byte value, string displayName)
            : base(value, displayName)
        {
        }

        public PlayerType(byte value)
            : base(value, value.ToString())
        {
        }
    }
}
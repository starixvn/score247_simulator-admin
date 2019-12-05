namespace Score247Management.Core.Teams.Models
{
    using MessagePack;
    using Score247Management.Core.Enumerations;

    [MessagePackObject(keyAsPropertyName: true)]
    public class Position : Enumeration
    {
        public static readonly Position Goalkeeper = new Position(1, "Goalkeeper");
        public static readonly Position RightBack = new Position(2, "Right back");
        public static readonly Position CentralDefender = new Position(3, "Central defender");
        public static readonly Position LeftBack = new Position(4, "Left back");
        public static readonly Position RightWinger = new Position(5, "Right winger");
        public static readonly Position CentralMidfielder = new Position(6, "Central midfielder");
        public static readonly Position LeftWinger = new Position(7, "Left winger");
        public static readonly Position Striker = new Position(8, "Striker");
        public static readonly Position Unknown = new Position(9, "Unknown");

        public Position()
        {
        }

        public Position(byte value, string displayName)
            : base(value, displayName)
        {
        }

        public Position(byte value)
            : base(value, value.ToString())
        {
        }
    }
}
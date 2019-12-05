namespace Score247Management.Core._Shared.Enumerations
{
    using MessagePack;
    using Score247Management.Core.Enumerations;

    [MessagePackObject(keyAsPropertyName: true)]
    public class LeagueRoundType : Enumeration
    {
        public const string Cup = "cup";
        public const string Group = "group";
        public const string PlayOff = "playoff";
        public const string Qualifier = "qualification";
        public const string Variable = "variable";

        public static readonly LeagueRoundType CupRound = new LeagueRoundType(1, Cup);
        public static readonly LeagueRoundType GroupRound = new LeagueRoundType(2, Group);
        public static readonly LeagueRoundType PlayOffRound = new LeagueRoundType(3, PlayOff);
        public static readonly LeagueRoundType QualifierRound = new LeagueRoundType(4, Qualifier);
        public static readonly LeagueRoundType VariableRound = new LeagueRoundType(5, Variable);

        public LeagueRoundType()
        {
        }

        public LeagueRoundType(byte value, string displayName)
            : base(value, displayName)
        {
        }
    }
}
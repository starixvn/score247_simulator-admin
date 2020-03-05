namespace Score247Management.Core._Shared.Enumerations
{
    using MessagePack;
    using Score247Management.Core.Enumerations;

    [MessagePackObject(keyAsPropertyName: true)]
    public class DescriptionType : Enumeration
    {
        //VAR Check Goal
        public static readonly DescriptionType Goal = new DescriptionType(1, "goal");

        //VAR Check Penalty
        public static readonly DescriptionType Penalty = new DescriptionType(2, "penalty");

        //VAR Check Red Card
        public static readonly DescriptionType RedCard = new DescriptionType(3, "red_card");

        //VAR Call Stage
        public static readonly DescriptionType CallStaged = new DescriptionType(4, "call_staged");

        //VAR Call Overturned
        public static readonly DescriptionType CallOverturned = new DescriptionType(5, "call_overturned");

        public DescriptionType()
        {
        }

        public DescriptionType(byte value, string displayName)
            : base(value, displayName)
        {
        }

        public bool IsCallStaged() => this == CallStaged;

        public bool IsCallOverturned() => this == CallOverturned;
    }
}
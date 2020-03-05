namespace Score247Management.Core._Shared.Enumerations
{
    using MessagePack;
    using Score247Management.Core.Enumerations;
    using System.Globalization;

    [MessagePackObject(keyAsPropertyName: true)]
    public class OutcomeType : Enumeration
    {
        private const char OutcomeSeperateChar = '_';
        private const string SpaceString = " ";

        //VAR Check Goal
        public static readonly OutcomeType Goal = new OutcomeType(1, "goal");

        //VAR Check Penalty
        public static readonly OutcomeType Penalty = new OutcomeType(2, "penalty");

        //VAR Check Red Card
        public static readonly OutcomeType RedCard = new OutcomeType(3, "red_card");

        //VAR Call Stage
        public static readonly OutcomeType NoGoal = new OutcomeType(4, "no_goal");

        //VAR Call Overturned
        public static readonly OutcomeType NoPenalty = new OutcomeType(5, "no_penalty");

        //VAR Call Overturned
        public static readonly OutcomeType NoRedCard = new OutcomeType(5, "no_red_card");

        public OutcomeType()
        {
        }

        public OutcomeType(byte value, string displayName)
            : base(value, displayName)
        {
        }

        public string CompiledDisplayName
        {
            get
            {
                var removedSeperatorName = string.Join(SpaceString, DisplayName.Split(OutcomeSeperateChar));
                return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(removedSeperatorName);
            }
        }
    }
}
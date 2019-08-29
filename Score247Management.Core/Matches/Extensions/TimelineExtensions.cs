namespace Score247Management.Core.Matches.Extensions
{
    using System.Collections.Generic;
    using Score247Management.Core._Shared.Enumerations;
    using Score247Management.Core.Matches.Models;

    public static class TimelineExtensions
    {
        public static List<byte> BasicEventTypes { get; }
           = new List<byte>
           {
                EventType.MatchStarted.Value,
                EventType.PeriodStart.Value,
                EventType.BreakStart.Value,
                EventType.ScoreChange.Value,
                EventType.YellowCard.Value,
                EventType.YellowRedCard.Value,
                EventType.RedCard.Value,
                EventType.PenaltyShootout.Value,
                EventType.MatchEnded.Value,
                EventType.InjuryTimeShown.Value,
                EventType.PenaltyMissed.Value
           };

        private const string NotTakenYetStatus = "not_taken_yet";

        public static bool IsScoreChangeInPenalty(this TimelineEvent timeline)
            => timeline.PeriodType != null
                && timeline.PeriodType.IsPenalties()
                && timeline.Type.IsScoreChange();

        public static bool IsShootOutInPenalty(this TimelineEvent timeline)
            => timeline.PeriodType != null
                && timeline.PeriodType.IsPenalties()
                && timeline.Type.IsPenaltyShootout()
                && timeline.PenaltyStatus?.ToLowerInvariant() != NotTakenYetStatus;

        public static bool IsBasicEvent(this TimelineEvent timeline)
        {
            return BasicEventTypes.Contains(timeline.Type.Value);
        }
    }
}
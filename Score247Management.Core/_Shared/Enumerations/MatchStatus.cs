namespace Score247Management.Core._Shared.Enumerations
{
    using MessagePack;
    using Score247Management.Core.Enumerations;

    [MessagePackObject]
    public class MatchStatus : Enumeration
    {
        ////not_started – The match is scheduled to be played
        public static readonly MatchStatus NotStarted = new MatchStatus(1, "not_started");

        ////postponed – The match has been postponed to a future date
        public static readonly MatchStatus Postponed = new MatchStatus(2, "postponed");

        ////start_delayed – The start of the match has been temporarily delayed
        public static readonly MatchStatus StartDelayed = new MatchStatus(3, "start_delayed");

        ////cancelled – The match has been canceled and will not be played
        public static readonly MatchStatus Cancelled = new MatchStatus(4, "cancelled");

        ////live – The match is currently in progress
        public static readonly MatchStatus Live = new MatchStatus(5, "live");

        ////1st_half – The match is in the first half
        public static readonly MatchStatus FirstHaft = new MatchStatus(6, "1st_half");

        ////2nd_half – The match is in the second half
        public static readonly MatchStatus SecondHaft = new MatchStatus(7, "2nd_half");

        ////overtime – The match is in overtime
        public static readonly MatchStatus Overtime = new MatchStatus(8, "overtime");

        ////1st_extra – The match is in the first extra period
        public static readonly MatchStatus FirstHaftExtra = new MatchStatus(9, "1st_extra");

        ////2nd_extra – The match is in the second extra period
        public static readonly MatchStatus SecondHaftExtra = new MatchStatus(10, "2nd_extra");

        ////awaiting_penalties – Waiting for announcement of penalties
        public static readonly MatchStatus AwaitingPenalties = new MatchStatus(11, "awaiting_penalties");

        ////penalties – Penalties are ongoing
        public static readonly MatchStatus Penalties = new MatchStatus(12, "penalties");

        ////pause – The match is paused
        public static readonly MatchStatus Pause = new MatchStatus(13, "pause");

        ////awaiting_extra_time – Waiting on referee to announce extra time
        public static readonly MatchStatus AwaitingExtraTime = new MatchStatus(14, "awaiting_extra_time");

        ////interrupted – The match has been interrupted
        public static readonly MatchStatus Interrupted = new MatchStatus(15, "interrupted");

        ////halftime – The match is in halftime
        public static readonly MatchStatus Halftime = new MatchStatus(16, "halftime");

        ////full-time – The match has ended
        public static readonly MatchStatus FullTime = new MatchStatus(17, "full-time");

        ////extra_time – Extra time has been added
        public static readonly MatchStatus ExtraTime = new MatchStatus(18, "extra_time");

        ////delayed – The match has been temporarily delayed and will be continued;
        public static readonly MatchStatus Delayed = new MatchStatus(19, "delayed");

        ////abandoned – The match has been abandoned
        public static readonly MatchStatus Abandoned = new MatchStatus(20, "abandoned");

        ////abandoned – The match has been abandoned
        public static readonly MatchStatus ExtraTimeHaftTime = new MatchStatus(21, "extra_time_halftime");

        ////ended – The match is over
        public static readonly MatchStatus Ended = new MatchStatus(22, "ended");

        ////closed – The match results have been confirmed
        public static readonly MatchStatus Closed = new MatchStatus(23, "closed");

        ////aet – The match has ended after extra time
        public static readonly MatchStatus EndedExtraTime = new MatchStatus(24, "aet");

        ////ap – The match has ended after penalties
        public static readonly MatchStatus EndedAfterPenalties = new MatchStatus(25, "ap");

        public MatchStatus()
        {
        }

        public MatchStatus(byte value, string displayName)
            : base(value, displayName)
        {
        }

        public MatchStatus(byte value)
            : base(value, value.ToString())
        {
        }

        public bool IsLive() => this == Live;

        public bool IsClosed() => this == Closed;

        public bool IsFirstHalf() => this == FirstHaft;

        public bool IsSecondHalf() => this == SecondHaft;

        public bool IsHalfTime() => this == Halftime;

        public bool IsFirstHaftExtra() => this == FirstHaftExtra;

        public bool IsSecondHalfExtra() => this == SecondHaftExtra;

        public bool IsNotStartOrDelay() => this == NotStarted || this == Postponed || this == Cancelled || this == StartDelayed;
    }
}
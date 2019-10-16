using Fanex.Data.Repository;

namespace Score247.Management.Site.Criterias.Match
{
    public class InsertTimelineLogCriteria : NonQueryCommand
    {
        public string Id { get; set; }
        public string RequestBody { get; set; }
        public string MatchId { get; set; }
        public long TimelineId { get; set; }
        public bool IsTimeline { get; set; }

        public override string GetSettingKey()
            => "TimelineLog_InsertLog";

        public override bool IsValid()
            => true;
    }
}
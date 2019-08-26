using Fanex.Data.Repository;

namespace Score247.Management.Site.Criterias.Match
{
    public class InsertTimelineLogCriteria : NonQueryCommand
    {
        public string Id { get; set; }
        public string Value { get; set; }
        public string MatchId { get; set; }
        public long TimelineId { get; set; }

        public override string GetSettingKey()
            => "TimelingLog_InsertLog";

        public override bool IsValid()
            => true;
    }
}

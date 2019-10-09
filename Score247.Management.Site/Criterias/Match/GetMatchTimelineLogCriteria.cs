using Fanex.Data.Repository;

namespace Score247.Management.Site.Criterias.Match
{
    public class GetMatchTimelineLogCriteria : CriteriaBase
    {
        public string Id { get; set; }

        public override string GetSettingKey()
            => "TimelineLog_GetByMatchId";

        public override bool IsValid()
            => true;
    }
}
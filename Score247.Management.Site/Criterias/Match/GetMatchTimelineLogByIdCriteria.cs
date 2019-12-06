using System;
using Fanex.Data.Repository;

namespace Score247.Management.Site.Criterias.Match
{
    public class GetMatchTimelineLogByIdCriteria : CriteriaBase
    {
        public GetMatchTimelineLogByIdCriteria(long timelineId)
        {
            TimelineId = timelineId;
        }

        public long TimelineId { get; }

        public override string GetSettingKey()
            => "TimelineLog_GetById";

        public override bool IsValid()
            => true;
    }
}
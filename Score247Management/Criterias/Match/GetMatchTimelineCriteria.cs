using System;
using Fanex.Data.Repository;

namespace Score247Management.Criterias.Match
{
    public class GetMatchTimelineCriteria : CriteriaBase
    {
        public string MatchId { get; set; }
        public override string GetSettingKey()
            => "Match_GetTimelineEvents";

        public override bool IsValid()
            => true;
    }
}

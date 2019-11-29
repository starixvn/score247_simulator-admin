using Fanex.Data.Repository;

namespace Score247.Management.Site.Criterias.Match
{
    public class DeleteTimelineByIdCriteria : NonQueryCommand
    {
        public string TimelineId { get; set; }

        public override string GetSettingKey()
            => "Match_DeleteTimelineById";

        public override bool IsValid()
            => TimelineId != "";
    }
}
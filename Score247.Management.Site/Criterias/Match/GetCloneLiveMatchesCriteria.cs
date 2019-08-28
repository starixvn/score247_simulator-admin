namespace Score247.Management.Site.Criterias.Match
{
    using System;
    using Fanex.Data.Repository;

    public class GetCloneLiveMatchesCriteria : CriteriaBase
    {
        public DateTime EventDate { get; set; }
        public int LimitMatch { get; set; }

        public override string GetSettingKey()
            => "LiveMatch_GetCloneMatches";

        public override bool IsValid()
            => true;
    }
}

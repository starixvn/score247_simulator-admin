namespace Score247.Management.Site.Criterias.Match
{
    using System;
    using Fanex.Data.Repository;

    public class MatchCloneCriteria : NonQueryCommand
    {
        public string EventDate { get; set; }
        public int LimitMatch { get; set; }

        public override string GetSettingKey()
            => "Match_CloneDistinct";

        public override bool IsValid()
            => true;
    }
}

namespace Score247.Management.Site.Criterias.Match
{
    using System.Collections.Generic;
    using Fanex.Data.Repository;

    public class DeleteMatchByIdCriteria : NonQueryCommand
    {
        public List<string> Ids { get; set; }

        public override string GetSettingKey()
            => "Match_DeleteByIds";

        public override bool IsValid()
            => Ids.Count > 0;
    }
}
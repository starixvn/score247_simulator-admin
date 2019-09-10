namespace Score247.Management.Site.Criterias.Match
{
    using System.Collections.Generic;
    using Fanex.Data.Repository;

    public class DeleteMatchByIdCriteria : NonQueryCommand
    {
        public string Id { get; set; }

        public override string GetSettingKey()
            => "Match_DeleteById";

        public override bool IsValid()
            => Id != "";
    }
}
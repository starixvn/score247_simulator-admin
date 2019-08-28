namespace Score247.Management.Site.Criterias.Match
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using Fanex.Data.Repository;

    public class DeleteMatchByDateCriteria : NonQueryCommand
    {
        [DataType(DataType.Date)]
        public DateTime DeleteDate { get; set; }
        public override string GetSettingKey()
            => "Match_DeleteMatchByDate";

        public override bool IsValid()
            => true;
    }
}

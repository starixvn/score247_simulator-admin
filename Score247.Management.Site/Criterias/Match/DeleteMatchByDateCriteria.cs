namespace Score247.Management.Site.Criterias.Match
{
    using System;
    using Fanex.Data.Repository;

    public class DeleteMatchByDateCriteria : NonQueryCommand
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }

        public int LimitCount { get; set; }

        public override string GetSettingKey()
            => "Admin_Match_DeleteMatchByDate";

        public override bool IsValid()
            => true;
    }
}
namespace Score247.Management.Site.Criterias.Match
{
    using System;
    using Fanex.Data.Repository;

    public class GetMatchByDateRangeCriteria : CriteriaBase
    {
        public int SportId { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public string Language { get; set; }
        public override string GetSettingKey()
         => "Match_GetByDateRange";

        public override bool IsValid()
         => true;
    }
}

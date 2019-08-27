namespace Score247.Management.Site.Criterias.Match
{
    using System;
    using Fanex.Data.Repository;

    public class GetMatchByDateRangeAndLeagueCriteria : CriteriaBase
    {
        public int SportId { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public string Language { get; set; }
        public string LeagueId { get; set; }
        public override string GetSettingKey()
         => "Match_GetByDateRangeAndLeague";

        public override bool IsValid()
         => true;
    }
}

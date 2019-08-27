namespace Score247.Management.Site.Criterias.Match
{
    using System;
    using Fanex.Data.Repository;
    public class InsertMatchCriteria : NonQueryCommand
    {
        public string Id { get; set; }
        public string MatchValue { get; set; }
        public string SportId { get; set; }
        public string MatchLanguage { get; set; }
        public string LeagueId { get; set; }
        public string EventDate { get; set; }
        public string Region { get; set; }

        public override string GetSettingKey()
            => "Match_InsertMatch";

        public override bool IsValid()
            => true;
    }
}

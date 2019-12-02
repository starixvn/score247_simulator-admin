using Fanex.Data.Repository;

namespace Score247.Management.Site.Criterias.League.Criterias
{
    public class GetLeagueSeasonIdCriteria : CriteriaBase
    {
        public GetLeagueSeasonIdCriteria(string leagueId)
        {
            LeagueId = leagueId;
        }

        public string LeagueId { get; }

        public override string GetSettingKey()
            => "Admin_League_GetLeagueSeasonId";

        public override bool IsValid()
            => !string.IsNullOrWhiteSpace(LeagueId);
    }
}
using Fanex.Data.Repository;

namespace Score247.Management.Site.Criterias.League
{
    public class RemoveLeagueSeasonCommand : NonQueryCommand
    {
        public RemoveLeagueSeasonCommand(string leagueId)
        {
            LeagueId = leagueId;
        }

        public string LeagueId { get; }

        public override string GetSettingKey()
            => "Admin_League_RemoveLeagueSeason";

        public override bool IsValid()
         => !string.IsNullOrWhiteSpace(LeagueId);
    }
}
using Fanex.Data.Repository;

namespace Score247.Management.Site.Criterias.Images
{
    public class UpdateRabitLeagueStatusCommand : NonQueryCommand
    {
        public UpdateRabitLeagueStatusCommand(int leagueId, bool status)
        {
            LeagueId = leagueId;
            IsFetchedTeams = status;
        }

        public int LeagueId { get; }

        public bool IsFetchedTeams { get; }

        public override string GetSettingKey()
            => "RapidLeague_UpdateStatus";

        public override bool IsValid()
            => true;
    }
}
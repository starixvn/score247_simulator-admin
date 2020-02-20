using Fanex.Data.Repository;

namespace Score247.Management.Site.Criterias.Images
{
    public class UpdateRabitTeamStatusCommand : NonQueryCommand
    {
        public UpdateRabitTeamStatusCommand(int teamId, bool status)
        {
            TeamId = teamId;
            IsFetched = status;
        }

        public int TeamId { get; }
        public bool IsFetched { get; }

        public override string GetSettingKey()
            => "RapidTeams_UpdateStatus";

        public override bool IsValid()
            => true;
    }
}
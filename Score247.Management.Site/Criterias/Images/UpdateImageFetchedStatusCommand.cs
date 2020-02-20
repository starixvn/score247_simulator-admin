using Fanex.Data.Repository;

namespace Score247.Management.Site.Criterias.Images
{
    public class UpdateImageFetchedStatusCommand : NonQueryCommand
    {
        public UpdateImageFetchedStatusCommand(int teamId)
        {
            TeamId = teamId;
            IsFetched = true;
        }

        public int TeamId { get; }

        public bool IsFetched { get; }

        public override string GetSettingKey()
            => "SportTeam_UpdateFetchStatus";

        public override bool IsValid()
            => true;
    }
}
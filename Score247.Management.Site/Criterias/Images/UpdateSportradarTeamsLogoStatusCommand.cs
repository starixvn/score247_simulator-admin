using Fanex.Data.Repository;

namespace Score247.Management.Site.Criterias.Images
{
    public class UpdateSportradarTeamsLogoStatusCommand : NonQueryCommand
    {
        public UpdateSportradarTeamsLogoStatusCommand(string teamId, bool isFetched)
        {
            TeamId = teamId;
            IsFetched = isFetched;
        }

        public string TeamId { get; }
        public bool IsFetched { get; }

        public override string GetSettingKey()
            => "SportradarTeam_UpdateFetchStatus";

        public override bool IsValid()
            => true;
    }
}
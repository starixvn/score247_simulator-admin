using Fanex.Data.Repository;

namespace Score247.Management.Site.Criterias.League
{
    public class UpdateActiveStatusCommand : NonQueryCommand
    {
        public bool IsActive { get; set; }
        public string LeagueId { get; set; }

        public override string GetSettingKey()
            => "Admin_League_UpdateActiveStatus";

        public override bool IsValid()
            => true;
    }
}
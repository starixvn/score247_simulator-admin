namespace Score247.Management.Site.Criterias.League
{
    using Fanex.Data.Repository;

    public class EnableAllLeaguesCommand : NonQueryCommand
    {
        public override string GetSettingKey()
            => "Admin_League_EnableAll";

        public override bool IsValid()
            => true;
    }
}
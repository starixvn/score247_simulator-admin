namespace Score247.Management.Site.Criterias.League
{
    using Fanex.Data.Repository;

    public class EnableMajorLeaguesCommand : NonQueryCommand
    {
        public override string GetSettingKey()
            => "Admin_League_EnableMajor";

        public override bool IsValid()
            => true;
    }
}
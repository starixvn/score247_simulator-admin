namespace Score247.Management.Site.Criterias.Match
{
    using Fanex.Data.Repository;

    public class DeleteLiveMatchCriteria : NonQueryCommand
    {
        public int LimitCount { get; set; }

        public override string GetSettingKey()
            => "Match_DeleteLiveMatch";

        public override bool IsValid()
            => true;
    }
}
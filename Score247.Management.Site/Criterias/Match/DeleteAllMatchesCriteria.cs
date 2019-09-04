using Fanex.Data.Repository;

namespace Score247.Management.Site.Criterias.Match
{
    public class DeleteAllMatchesCriteria : NonQueryCommand
    {
        public override string GetSettingKey()
            => "Match_DeleteAll";

        public override bool IsValid()
            => true;
    }
}
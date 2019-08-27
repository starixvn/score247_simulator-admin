using Fanex.Data.Repository;

namespace Score247.Management.Site.Criterias.Match
{
    public class DeleteMatchByDateCriteria : NonQueryCommand
    {
        public string DeleteDate { get; set; }
        public override string GetSettingKey()
            => "Match_DeleteMatchByDate";

        public override bool IsValid()
            => true;
    }
}

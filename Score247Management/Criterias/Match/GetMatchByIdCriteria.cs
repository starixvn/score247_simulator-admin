using Fanex.Data.Repository;

namespace Score247Management.Criterias.Match
{
    public class GetMatchByIdCriteria : CriteriaBase
    {
        public string Id { get; set; }
        public string Language { get; set; }
        public override string GetSettingKey()
            => "Match_GetById";

        public override bool IsValid()
            => true;
    }
}

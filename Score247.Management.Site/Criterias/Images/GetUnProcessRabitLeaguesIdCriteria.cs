using Fanex.Data.Repository;

namespace Score247.Management.Site.Criterias.Images
{
    public class GetUnProcessRabitLeaguesIdCriteria : CriteriaBase
    {
        public override string GetSettingKey()
            => "RabitLeague_GetUnprocess";

        public override bool IsValid()
            => true;
    }
}
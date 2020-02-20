using Fanex.Data.Repository;

namespace Score247.Management.Site.Criterias.Images
{
    public class GetSportradarTeamsLogoUnFetchedCriteria : CriteriaBase
    {
        public override string GetSettingKey()
            => "SportradarTeamsLogo_GetUnFetched";

        public override bool IsValid()
            => true;
    }
}
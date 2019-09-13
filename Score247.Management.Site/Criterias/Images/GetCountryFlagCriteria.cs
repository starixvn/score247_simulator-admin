namespace Score247.Management.Site.Criterias.Images
{
    using Fanex.Data.Repository;

    public class GetCountryFlagCriteria : CriteriaBase
    {
        public string ShortName { get; set; }

        public override string GetSettingKey()
            => "Image_GetCountryImages";

        public override bool IsValid()
            => true;
    }
}
namespace Score247.Management.Site.Criterias.Team
{
    using Fanex.Data.Repository;

    public class GetTeamByIdCriteria : CriteriaBase
    {
        public string TeamId { get; set; }

        public override string GetSettingKey()
            => "Team_GetById";

        public override bool IsValid()
            => true;
    }
}
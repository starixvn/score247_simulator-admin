namespace Score247Management.Core.Images
{
    public class SportTeam
    {
        public SportTeam(int teamId, string teamName)
        {
            TeamId = teamId;
            TeamName = teamName;
        }

        public int TeamId { get; }
        public string TeamName { get; }
    }
}
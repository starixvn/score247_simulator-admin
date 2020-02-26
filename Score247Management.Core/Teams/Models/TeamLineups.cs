namespace Score247Management.Core.Teams.Models
{
    using System.Collections.Generic;
    using System.Linq;
    using MessagePack;
    using Newtonsoft.Json;

    [MessagePackObject(keyAsPropertyName: true)]
    public class TeamLineups : Team
    {
        [SerializationConstructor, JsonConstructor]
        public TeamLineups(
            string id,
            string name,
            bool isHome,
            Coach coach,
            string formation,
            IEnumerable<Player> players,
            IEnumerable<Player> substitutions)
            : base(id, name, string.Empty, string.Empty, string.Empty, isHome, default(TeamStatistic), string.Empty)
        {
            Coach = coach;
            Formation = formation;
            Players = players;
            Substitutions = substitutions;
        }

        public Coach Coach { get; set; }

        public string Formation { get; set; }

        public IEnumerable<Player> Players { get; set; }

        public IEnumerable<Player> Substitutions { get; set; }
    }
}
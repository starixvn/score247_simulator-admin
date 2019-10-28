namespace Score247Management.Core.Teams.Models
{
    using System.Collections.Generic;
    using System.Linq;
    using MessagePack;
    using Newtonsoft.Json;

    [MessagePackObject(keyAsPropertyName: true)]
    public class TeamLineups : Team
    {
        private const char formationSplitChar = '-';

        [SerializationConstructor, JsonConstructor]
        public TeamLineups(
            string id,
            string name,
            string country,
            string countryCode,
            string flag,
            bool isHome,
            TeamStatistic statistic,
            string abbreviation,
            Coach coach,
            string formation,
            IEnumerable<Player> players,
            IEnumerable<Player> substitutions) : base(id, name, country, countryCode, flag, isHome, statistic, abbreviation)
        {
            Coach = coach;
            Formation = formation;
            Players = players;
            Substitutions = substitutions;
        }

        public Coach Coach { get; }

        public string Formation { get; }

        public IEnumerable<Player> Players { get; }

        public IEnumerable<Player> Substitutions { get; }

        public IList<byte> ConvertFormationToList()
        {
            var tempList = (Formation ?? string.Empty)
           .Split(formationSplitChar)
           .Where(fm => !string.IsNullOrWhiteSpace(fm))
           .Select(fm => byte.Parse(fm))
           .ToList();
            var formationList = new List<byte> { 1 };
            formationList.AddRange(tempList);

            return formationList;
        }

        public byte CountTotalFormationLine() => (byte)(1 + ConvertFormationToList().Count);
    }
}
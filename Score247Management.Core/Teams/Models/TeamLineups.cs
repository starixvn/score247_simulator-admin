namespace Score247Management.Core.Teams.Models
{
    using System.Collections.Generic;
    using System.Linq;
    using MessagePack;

    [MessagePackObject(keyAsPropertyName: true)]
    public class TeamLineups : Team
    {
        private const char formationSplitChar = '-';

        public TeamLineups(string value) : base(value)
        {
        }

        public Coach Coach { get; set; }

        public string Formation { get; set; }

        public IEnumerable<Player> Players { get; set; }

        public IEnumerable<Player> Substitutions { get; set; }

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
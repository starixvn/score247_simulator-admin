namespace Score247Management.Core.Odds.Models
{
    using MessagePack;

    [MessagePackObject(keyAsPropertyName: true)]
    public class Bookmaker
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public override int GetHashCode()
            => Id.GetHashCode();

        public override bool Equals(object obj)
        {
            var actualObj = obj as Bookmaker;
            if (actualObj == null)
            {
                return false;
            }

            return Id == actualObj.Id
                && Name == actualObj.Name;
        }
    }
}
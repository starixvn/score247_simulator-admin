namespace Score247Management.Core.Odds.Models
{
    public class Bookmaker
    {
        public Bookmaker(string id, string name)
        {
            Id = id;
            Name = name;
        }

        public string Id { get; }

        public string Name { get; }

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
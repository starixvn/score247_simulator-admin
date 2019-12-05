namespace Score247Management.Core._Shared.Enumerations
{
    using MessagePack;
    using Score247Management.Core.Enumerations;

    [MessagePackObject(keyAsPropertyName: true)]
    public class Language : Enumeration
    {
        public static readonly Language en_US = new Language(1, "en-US");

        public Language()
        {
        }

        private Language(byte value, string displayName)
            : base(value, displayName)
        {
        }
    }
}
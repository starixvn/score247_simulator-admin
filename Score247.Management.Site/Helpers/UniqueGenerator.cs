namespace Score247.Management.Site.Helpers
{
    using System;

    public static class UniqueGenerator
    {
        public static string GetIdWithPattern(string pattern)
        {
            return pattern + Guid.NewGuid().GetHashCode().ToString();
        }

        public static int GetIntId()
        {
            return Guid.NewGuid().GetHashCode();
        }
    }


}

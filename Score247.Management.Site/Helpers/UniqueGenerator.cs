namespace Score247.Management.Site.Helpers
{
    using System;

    public static class UniqueGenerator
    {
        public static string GetIdWithPattern(string pattern)
        {
            return pattern + Math.Abs(DateTime.Now.Ticks).ToString();
        }

        public static int GetIntId()
        {
            //return Guid.NewGuid().GetHashCode();
            return Math.Abs((int)DateTime.Now.Ticks);
        }

        public static long GetLongId()
        {
            return Math.Abs(DateTime.Now.Ticks);
        }
    }
}
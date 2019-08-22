namespace Score247.Management.Site.Helpers
{
    using System;

    public static class UniqueGenerator
    {
        public static string GetIdWithPattern(string pattern)
        {
            return pattern + DateTime.Now.Ticks.ToString();
        }

        public static int GetIntId()
        {
            //return Guid.NewGuid().GetHashCode();
            return (int)DateTime.Now.Ticks;
        }
    }


}

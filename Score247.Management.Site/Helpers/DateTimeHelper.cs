using System;

namespace Score247.Management.Site.Helpers
{
    public static class DateTimeHelper
    {
        public static DateTime GetBeginingOfDate(DateTime inputTime)
        {
            return new DateTime(inputTime.Year, inputTime.Month, inputTime.Day, 0, 0, 0);
        }
    }
}
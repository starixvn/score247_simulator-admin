using System;

namespace Score247.Management.Site.Helpers
{
    public static class DateTimeHelper
    {
        public static DateTime GetBeginingOfDate(DateTime inputTime)
        {
            return new DateTime(inputTime.Year, inputTime.Month, inputTime.Day, 0, 0, 0);
        }

        public static DateTime ConvertLocalTimeToTimezone(DateTime inputTime, TimeSpan offset)
        {
            var utcFromDate = new DateTime(inputTime.Year, inputTime.Month, inputTime.Day, inputTime.Hour, inputTime.Minute, inputTime.Second, DateTimeKind.Utc);

            return utcFromDate.Add(offset);
        }
    }
}
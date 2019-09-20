namespace Score247.Management.Site.Helpers
{
    using System;
    using Microsoft.AspNetCore.Http;

    public static class DateTimeHelper
    {
        public static DateTime GetBeginingOfDate(DateTime inputTime)
        {
            return new DateTime(inputTime.Year, inputTime.Month, inputTime.Day, 0, 0, 0);
        }

        public static TimeSpan GetTimezone(ISession session)
        {
            var result = session.GetString("Timezone");
            if (string.IsNullOrEmpty(result))
            {
                result = "07:00:00";
            }

            TimeSpan.TryParse(result, out TimeSpan offset);

            return offset;
        }

        public static DateTime ConvertLocalTimeToTimezone(DateTime inputTime, TimeSpan offset)
        {
            var utcFromDate = new DateTime(inputTime.Year, inputTime.Month, inputTime.Day, inputTime.Hour, inputTime.Minute, inputTime.Second, DateTimeKind.Utc);

            return utcFromDate.Add(offset);
        }
    }
}
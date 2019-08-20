namespace Score247.Management.Site.Helpers
{
    using Newtonsoft.Json;
    using Score247Management.Core.Matches.Models;

    public static class TimelineValueConverter
    {
        public static TimelineEvent DeserialValue(this TimelineEvent match)
        {
            var value = match.Value;
            var result = JsonConvert.DeserializeObject<TimelineEvent>(value);
            result.Value = value;

            return result;
        }
    }
}

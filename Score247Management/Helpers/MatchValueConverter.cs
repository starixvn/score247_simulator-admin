using Newtonsoft.Json;
using Score247Management.Core.Matches.Models;

namespace Score247Management.Helpers
{
    public static class MatchValueConverter
    {
        public static string ConvertToValue(this Match match)
        {
            var value = JsonConvert.SerializeObject(match);

            return value;
        }

        public static Match DeserialValue(this Match match)
        {
            var value = match.Value;
            var result = JsonConvert.DeserializeObject<Match>(value);
            result.Value = value;

            return result;
        }
    }
}

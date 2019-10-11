using System;
using System.Linq;
using Newtonsoft.Json;
using Score247Management.Core.Matches.Models;

namespace Score247.Management.Site.Helpers
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
            for (var i = 0; i < result.Teams.Count(); i++)
            {
                result.Teams.ElementAt(i).qualifier = result.Teams.ElementAt(i).IsHome ? "home" : "away";
            }
            result.Value = value;

            return result;
        }

        public static Match DeserialValue(this Match match, TimeSpan offset)
        {
            var value = match.Value;
            var result = JsonConvert.DeserializeObject<Match>(value);
            for (var i = 0; i < result.Teams.Count(); i++)
            {
                result.Teams.ElementAt(i).qualifier = result.Teams.ElementAt(i).IsHome ? "home" : "away";
            }
            result.EventDate = result.EventDate.ToUniversalTime().Add(offset);
            result.Value = value;

            return result;
        }
    }
}
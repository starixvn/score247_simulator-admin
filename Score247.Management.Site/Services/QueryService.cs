using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using MessagePack;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Score247Management.Core.Matches.Models;

namespace Score247.Management.Site.Services
{
    public interface IQueryService
    {
        Task<IEnumerable<MatchSummary>> GetLiveMatches(string language);

        Task<IEnumerable<MatchSummary>> GetMatchesByDateRange(DateTimeOffset fromDate, DateTimeOffset toDate, string language, TimeSpan offset);
    }

    public class QueryService : IQueryService
    {
        private readonly IConfiguration configuration;

        public QueryService(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public async Task<IEnumerable<MatchSummary>> GetLiveMatches(string language)
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(configuration.GetValue<string>("QueryApi:Url"));

                client.DefaultRequestHeaders.Add("Accept", "application/x-msgpack");

                var requestPath = $"api/soccer/{language}/matches/live";
                var responseMessage = await client.GetAsync(requestPath);

                if (responseMessage.IsSuccessStatusCode)
                {
                    try
                    {
                        var bytes = await responseMessage.Content.ReadAsByteArrayAsync();

                        return MessagePackSerializer.Deserialize<IEnumerable<MatchSummary>>(bytes);

                        //var matchData = await responseMessage.Content.ReadAsStringAsync();
                        //return JsonConvert.DeserializeObject<IEnumerable<MatchSummary>>(matchData);
                    }
                    catch (Exception)
                    {
                    }
                }
            }

            return new List<MatchSummary>();
        }

        public async Task<IEnumerable<MatchSummary>> GetMatchesByDateRange(DateTimeOffset fromDate, DateTimeOffset toDate, string language, TimeSpan offset)
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(configuration.GetValue<string>("QueryApi:Url"));

                client.DefaultRequestHeaders.Add("Accept", "application/x-msgpack");
                var query = new Dictionary<string, string>
                {
                    ["fd"] = fromDate.ToString("yyyy-MM-ddTHH:mm:sszzz"),
                    ["td"] = toDate.ToString("yyyy-MM-ddTHH:mm:sszzz")
                };
                var requestPath = $"api/soccer/{language}/matches";
                var responseMessage = await client.GetAsync(QueryHelpers.AddQueryString(requestPath, query));

                if (responseMessage.IsSuccessStatusCode)
                {
                    try
                    {
                        var bytes = await responseMessage.Content.ReadAsByteArrayAsync();
                        var matches = MessagePackSerializer.Deserialize<IEnumerable<MatchSummary>>(bytes);

                        //var matchData = await responseMessage.Content.ReadAsStringAsync();
                        //var matches = JsonConvert.DeserializeObject<IEnumerable<MatchSummary>>(matchData);
                        return matches.Select(match =>
                        {
                            match.DisplayDate = match.EventDate.Add(offset).ToString("dd/MM/yyyy HH:mm:ss");
                            return match;
                        });
                    }
                    catch (Exception)
                    {
                    }
                }
            }

            return new List<MatchSummary>();
        }
    }
}
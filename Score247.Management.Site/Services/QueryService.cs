using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using MessagePack;
using Microsoft.Extensions.Configuration;
using Score247Management.Core.Matches.Models;

namespace Score247.Management.Site.Services
{
    public interface IQueryService
    {
        Task<IEnumerable<MatchSummary>> GetLiveMatches(string language);
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

                var requestPath = $"api/soccer/{language}/matches/live";
                var responseMessage = await client.GetAsync(requestPath);

                if (responseMessage.IsSuccessStatusCode)
                {
                    try
                    {
                        var bytes = await responseMessage.Content.ReadAsByteArrayAsync();

                        return MessagePackSerializer.Deserialize<IEnumerable<MatchSummary>>(bytes);
                    }
                    catch (Exception ex)
                    {
                    }
                }
            }

            return new List<MatchSummary>();
        }
    }
}
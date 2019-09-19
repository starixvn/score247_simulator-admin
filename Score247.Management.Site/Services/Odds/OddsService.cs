using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using MessagePack;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Score247.Management.Site.Repositories.Matches;
using Score247.Management.Site.ViewModels.Matches;
using Score247Management.Core.Odds.Models;

namespace Score247.Management.Site.Services.Odds
{
    public interface IOddsService
    {
        Task<MatchOdds> GetMatchOdds(string matchId, int betTypeId, string language, string format);

        Task<OddsEventViewModel> CreateOddsViewModel(string matchId, TimeSpan offset);
    }

    public class OddsService : IOddsService
    {
        private IConfiguration configuration { get; }

        public OddsService(IConfiguration config)
        {
            configuration = config;
        }

        public async Task<OddsEventViewModel> CreateOddsViewModel(string matchId, TimeSpan offset)
        {
            var repository = new MatchRepository();
            var match = await repository.GetMatchById(matchId, offset);
            var viewModel = new OddsEventViewModel();
            viewModel.Match = match;
            viewModel.MatchOdds = await GetMatchOdds(matchId, 1, "en-US", "des");
            return viewModel;
        }

        public async Task<MatchOdds> GetMatchOdds(string matchId, int betTypeId, string language, string format)
        {
            using (var httpClient = new HttpClient())
            {
                httpClient.BaseAddress = new Uri(configuration.GetValue<string>("QueryApi:Url"));
                var requestPath = $"api/soccer/{language}/odds/{matchId}/{betTypeId}/{format}";

                var responseMessage = await httpClient.GetAsync(requestPath);

                if (responseMessage.IsSuccessStatusCode)
                {
                    var bytes = await responseMessage.Content.ReadAsByteArrayAsync();

                    return MessagePackSerializer.Deserialize<MatchOdds>(bytes);
                }

                return new MatchOdds();
            }
        }
    }
}
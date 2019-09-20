using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using MessagePack;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Score247.Management.Site.Helpers;
using Score247.Management.Site.Repositories.Matches;
using Score247.Management.Site.ViewModels.Matches;
using Score247.Management.Site.ViewModels.Odds;
using Score247Management.Core.Odds.Dtos;
using Score247Management.Core.Odds.Models;

namespace Score247.Management.Site.Services.Odds
{
    public interface IOddsService
    {
        Task<MatchOdds> GetMatchOdds(string matchId, int betTypeId, string language, string format);

        Task<MatchOddsMovement> GetMatchOddsMovement(string matchId, int betTypeId, string bookmakerId, string language, string format);

        Task<OddsEventViewModel> CreateOddsViewModel(string matchId, TimeSpan offset);

        Task<bool> Send1X2OddsEvent(OddsChange1X2RequestModel request, TimeSpan offset);
    }

    public class OddsService : IOddsService
    {
        private IConfiguration configuration { get; }
        private ISimulatorService simulatorService { get; }
        private ITriggerJobService triggerService { get; }

        public OddsService(IConfiguration config, ISimulatorService simulatorService, ITriggerJobService trigger)
        {
            configuration = config;
            this.simulatorService = simulatorService;
            triggerService = trigger;
        }

        public async Task<OddsEventViewModel> CreateOddsViewModel(string matchId, TimeSpan offset)
        {
            var repository = new MatchRepository();
            var match = await repository.GetMatchById(matchId, offset);
            var viewModel = new OddsEventViewModel();
            viewModel.Match = match;
            viewModel.MatchOdds = await GetMatchOdds(matchId, 1, "en-US", "des");
            viewModel.MatchOddsMovement = new MatchOddsMovement();

            if (viewModel.MatchOdds.IsBetTypeOddsListEmpty())
            {
                return viewModel;
            }

            viewModel.MatchOddsMovement = await GetMatchOddsMovement(matchId, 1, viewModel.MatchOdds.BetTypeOddsList.FirstOrDefault().Bookmaker.Id, "en-US", "des");

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
            }

            return new MatchOdds();
        }

        public async Task<MatchOddsMovement> GetMatchOddsMovement(string matchId, int betTypeId, string bookmakerId, string language, string format)
        {
            using (var httpClient = new HttpClient())
            {
                httpClient.BaseAddress = new Uri(configuration.GetValue<string>("QueryApi:Url"));
                var requestPath = $"api/soccer/{language}/odds-movement/{matchId}/{betTypeId}/{format}/{bookmakerId}";

                var responseMessage = await httpClient.GetAsync(requestPath);

                if (responseMessage.IsSuccessStatusCode)
                {
                    var bytes = await responseMessage.Content.ReadAsByteArrayAsync();

                    return MessagePackSerializer.Deserialize<MatchOddsMovement>(bytes);
                }
            }

            return new MatchOddsMovement();
        }

        public async Task<bool> Send1X2OddsEvent(OddsChange1X2RequestModel request, TimeSpan offset)
        {
            var matchRepo = new MatchRepository();
            var match = await matchRepo.GetMatchById(request.MatchId);

            var outcomes = new List<Outcome>();
            outcomes.Add(new Outcome()
            {
                odds_field_id = 3,
                type = "home",
                odds = request.HomeOdds,
                opening_odds = request.HomeOdds,
                deep_link = "http://stats.betradar.com/tb.php?type=iscreenFromId&id=9192052710",
                odds_trend = "down"
            });

            outcomes.Add(new Outcome()
            {
                odds_field_id = 4,
                type = "draw",
                odds = request.DrawOdds,
                opening_odds = request.DrawOdds,
                deep_link = "http://stats.betradar.com/tb.php?type=iscreenFromId&id=9192052710",
                odds_trend = "down"
            });

            outcomes.Add(new Outcome()
            {
                odds_field_id = 5,
                type = "away",
                odds = request.AwayOdds,
                opening_odds = request.AwayOdds,
                deep_link = "http://stats.betradar.com/tb.php?type=iscreenFromId&id=9192052710",
                odds_trend = "down"
            });

            var books = new List<Book>();
            books.Add(new Book()
            {
                id = request.BookmakerId,
                name = request.BookmakerId,
                outcomes = outcomes
            });

            var sportEvent = new SportEvent()
            {
                id = request.MatchId,
                scheduled = match.EventDate,
                markets_last_updated = DateTimeHelper.ConvertLocalTimeToTimezone(request.LastUpdate, offset),
                markets = new List<Market>()
                {
                    new Market()
                    {
                        odds_type_id = 2,
                        name = "3way",
                        group_name = "regular",
                        books = books
                    }
                }
            };
            var sportEvents = new List<SportEvent>() { sportEvent };

            var scheduleOddsRequest = new OddsScheduleDto()
            {
                sport_events = sportEvents
            };

            await simulatorService.SendScheduleOdds(scheduleOddsRequest);

            await triggerService.TriggerFetchOddsJobs();

            return true;
        }
    }
}
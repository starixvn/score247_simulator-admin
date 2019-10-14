using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using MessagePack;
using Microsoft.Extensions.Configuration;
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

        Task<MatchOddsMovement> GetMatchOddsMovement(string matchId, int betTypeId, string bookmakerId, string language, string format, TimeSpan offset);

        Task<OddsEventViewModel> CreateOddsViewModel(string matchId, TimeSpan offset);

        Task<bool> Send1X2OddsEvent(OddsChangeRequestModel request, TimeSpan offset);

        Task SendOddsEvent(OddsChangeRequestModel oddsChange, TimeSpan timeSpan);
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
            var viewModel = new OddsEventViewModel
            {
                Match = match,
                MatchOdds = await GetMatchOdds(matchId, 1, "en-US", "des"),
                MatchOddsMovement = new MatchOddsMovement()
            };

            if (viewModel.MatchOdds.IsBetTypeOddsListEmpty())
            {
                return viewModel;
            }

            viewModel.MatchOddsMovement = await GetMatchOddsMovement(matchId, 1, viewModel.MatchOdds.BetTypeOddsList.FirstOrDefault().Bookmaker.Id, "en-US", "des", offset);

            return viewModel;
        }

        public async Task<MatchOdds> GetMatchOdds(string matchId, int betTypeId, string language, string format)
        {
            using (var httpClient = new HttpClient())
            {
                httpClient.BaseAddress = new Uri(configuration.GetValue<string>("QueryApi:Url"));
                httpClient.DefaultRequestHeaders.Add("Accept", "application/x-msgpack");
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

        public async Task<MatchOddsMovement> GetMatchOddsMovement(string matchId, int betTypeId, string bookmakerId, string language, string format, TimeSpan offset)
        {
            using (var httpClient = new HttpClient())
            {
                httpClient.BaseAddress = new Uri(configuration.GetValue<string>("QueryApi:Url"));
                httpClient.DefaultRequestHeaders.Add("Accept", "application/x-msgpack");

                var requestPath = $"api/soccer/{language}/odds-movement/{matchId}/{betTypeId}/{format}/{bookmakerId}";

                var responseMessage = await httpClient.GetAsync(requestPath);

                if (!responseMessage.IsSuccessStatusCode) return new MatchOddsMovement();
                var bytes = await responseMessage.Content.ReadAsByteArrayAsync();

                var result = MessagePackSerializer.Deserialize<MatchOddsMovement>(bytes);
                result.OddsMovements = result.OddsMovements.Select(x => { x.DisplayUpdateTime = x.UpdateTime.Add(offset).ToString("yyyy/MM/dd HH:mm:ss"); return x; });

                return result;
            }

            return new MatchOddsMovement();
        }

        public async Task<bool> Send1X2OddsEvent(OddsChangeRequestModel request, TimeSpan offset)
        {
            var scheduleOddsRequest = CreateOddsRequest();
            var sportEvent = await CreateOddSportEvent(request, offset, "3way", 2);
            sportEvent.markets[0].books[0].outcomes = Create1X2OutCome(request).ToList();

            scheduleOddsRequest.sport_events.Add(sportEvent);

            await simulatorService.SendScheduleOdds(scheduleOddsRequest);

            await triggerService.TriggerFetchOddsJobs();

            return true;
        }

        private OddsScheduleDto CreateOddsRequest()
        {
            var sportEvents = new List<SportEvent>() { };

            var scheduleOddsRequest = new OddsScheduleDto()
            {
                sport_events = sportEvents
            };
            return scheduleOddsRequest;
        }

        private static IEnumerable<Outcome> Create1X2OutCome(OddsChangeRequestModel request)
        {
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
            return outcomes;
        }

        public async Task SendOddsEvent(OddsChangeRequestModel oddsChange, TimeSpan timeSpan)
        {
            oddsChange = await GetOpeningOdds(oddsChange);
            if (oddsChange.BetType == 1)
            {
                await Send1X2OddsEvent(oddsChange, timeSpan);
            }
            else if (oddsChange.BetType == 2)
            {
                await SendOverUnderOddsEvent(oddsChange, timeSpan);
            }
            else
            {
                await SendAsianHDPOddsEvent(oddsChange, timeSpan);
            }
        }

        private async Task<bool> SendAsianHDPOddsEvent(OddsChangeRequestModel request, TimeSpan offset)
        {
            var scheduleOddsRequest = CreateOddsRequest();
            var sportEvent = await CreateOddSportEvent(request, offset, "asian_handicap", 5);
            sportEvent.markets[0].books[0].outcomes = CreateAsianHDPOutCome(request).ToList();

            scheduleOddsRequest.sport_events.Add(sportEvent);

            await simulatorService.SendScheduleOdds(scheduleOddsRequest);

            await triggerService.TriggerFetchOddsJobs();

            return true;
        }

        private IEnumerable<Outcome> CreateAsianHDPOutCome(OddsChangeRequestModel request)
        {
            var outcomes = new List<Outcome>
            {
                new Outcome()
                {
                    odds_field_id = 7,
                    type = "home",
                    odds = request.HomeHDPOdds,
                    opening_odds = request.HomeHDPOdds,
                    handicap = request.HDPValue,
                    deep_link = "http://stats.betradar.com/tb.php?type=iscreenFromId&id=9192052710",
                    odds_trend = "down"
                },
                new Outcome()
                {
                    odds_field_id = 8,
                    type = "away",
                    odds = request.AwayHDPOdds,
                    opening_odds = request.AwayHDPOdds,
                    handicap = request.HDPValue,
                    deep_link = "http://stats.betradar.com/tb.php?type=iscreenFromId&id=9192052710",
                    odds_trend = "down"
                }
            };

            return outcomes;
        }

        private async Task<bool> SendOverUnderOddsEvent(OddsChangeRequestModel request, TimeSpan offset)
        {
            var scheduleOddsRequest = CreateOddsRequest();
            var sportEvent = await CreateOddSportEvent(request, offset, "total", 3);
            sportEvent.markets[0].books[0].outcomes = CreateOverUnderOutCome(request).ToList();

            scheduleOddsRequest.sport_events.Add(sportEvent);

            await simulatorService.SendScheduleOdds(scheduleOddsRequest);

            await triggerService.TriggerFetchOddsJobs();

            return true;
        }

        private IEnumerable<Outcome> CreateOverUnderOutCome(OddsChangeRequestModel request)
        {
            var outcomes = new List<Outcome>
            {
                new Outcome()
                {
                    odds_field_id = 7,
                    type = "over",
                    odds = request.OverOdds,
                    opening_odds = request.OverOdds,
                    total = request.TotalOUOdds,
                    opening_total = request.TotalOUOdds,
                    deep_link = "http://stats.betradar.com/tb.php?type=iscreenFromId&id=9192052710",
                    odds_trend = "down"
                },
                new Outcome()
                {
                    odds_field_id = 8,
                    type = "under",
                    odds = request.UnderOdds,
                    opening_odds = request.UnderOdds,
                    total = request.TotalOUOdds,
                    opening_total = request.TotalOUOdds,
                    deep_link = "http://stats.betradar.com/tb.php?type=iscreenFromId&id=9192052710",
                    odds_trend = "down"
                }
            };

            return outcomes;
        }

        private async Task<SportEvent> CreateOddSportEvent(OddsChangeRequestModel request, TimeSpan offset, string oddsName, int oddsTypeId)
        {
            var matchRepo = new MatchRepository();
            var match = await matchRepo.GetMatchById(request.MatchId);

            var books = new List<Book>
            {
                new Book()
                {
                    id = request.BookmakerId,
                    name = request.BookmakerId,
                    outcomes = new List<Outcome>()
                }
            };

            var sportEvent = new SportEvent()
            {
                id = request.MatchId,
                scheduled = match.EventDate,
                markets_last_updated = DateTimeHelper.ConvertLocalTimeToTimezone(request.LastUpdate, offset),
                markets = new List<Market>()
                {
                    new Market()
                    {
                        odds_type_id = oddsTypeId,
                        name = oddsName,
                        group_name = "regular",
                        books = books
                    }
                }
            };
            return sportEvent;
        }

        private async Task<OddsChangeRequestModel> GetOpeningOdds(OddsChangeRequestModel oddsChangeRequest)
        {
            var matchOdds = await GetMatchOdds(oddsChangeRequest.MatchId, oddsChangeRequest.BetType, "en-US", "des");
            if (matchOdds.BetTypeOddsList.Any())
            {
                var firstOdds = matchOdds.BetTypeOddsList.FirstOrDefault(x => x.Bookmaker.Id == oddsChangeRequest.BookmakerId);

                if (firstOdds != null)
                {
                    oddsChangeRequest.OpeningHomeOdds = firstOdds.BetOptions.FirstOrDefault(x => x.Type == "home")?.OpeningOdds.ToString();
                    oddsChangeRequest.OpeningDrawOdds = firstOdds.BetOptions.FirstOrDefault(x => x.Type == "draw")?.OpeningOdds.ToString();
                    oddsChangeRequest.OpeningAwayOdds = firstOdds.BetOptions.FirstOrDefault(x => x.Type == "away")?.OpeningOdds.ToString();
                    oddsChangeRequest.OpeningHomeHDPOdds = firstOdds.BetOptions.FirstOrDefault(x => x.Type == "home")?.OpeningOdds.ToString();
                    oddsChangeRequest.OpeningAwayHDPOdds = firstOdds.BetOptions.FirstOrDefault(x => x.Type == "away")?.OpeningOdds.ToString();
                    oddsChangeRequest.OpeningHDPValue = firstOdds.BetOptions.FirstOrDefault(x => x.Type == "home")?.OpeningOptionValue.ToString();
                    oddsChangeRequest.OpeningOverOdds = firstOdds.BetOptions.FirstOrDefault(x => x.Type == "over")?.OpeningOdds.ToString();
                    oddsChangeRequest.OpeningUnderOdds = firstOdds.BetOptions.FirstOrDefault(x => x.Type == "under")?.OpeningOdds.ToString();
                    oddsChangeRequest.OpeningTotalOUOdds = firstOdds.BetOptions.FirstOrDefault(x => x.Type == "over")?.OpeningOptionValue.ToString();
                }
            }
            return oddsChangeRequest;
        }
    }
}
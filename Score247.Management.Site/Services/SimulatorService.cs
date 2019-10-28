namespace Score247.Management.Site.Services
{
    using Fanex.Logging;
    using System;
    using System.IO;
    using System.Net.Http;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Configuration;
    using Newtonsoft.Json;
    using Score247.Management.Site.Models.Events;
    using Score247.Management.Site.Repositories.Matches;
    using Score247Management.Core.Odds.Dtos;
    using Score247.Management.Site.Models;

    public interface ISimulatorService
    {
        Task SendCleanExistData();

        Task SendCleanTimelines();

        Task<bool> SendScheduleOdds(OddsScheduleDto oddsSchedule);

        Task<HttpResponseMessage> SendDailyResult(DailyResultDto dailyResultDto);

        Task<HttpResponseMessage> SendLiveResult(DailyResultDto dailyResultDto);

        Task<HttpResponseMessage> SendDailySchedule(DailyScheduleDto dailyResultDto);

        Task<DailyResultDto> GetDailyResult(DateTime resultDate);

        Task<DailyScheduleDto> GetDailySchedule(DateTime scheduleDate);

        Task<DailyResultDto> GetLiveResult();

        Task SendMatchEvent(MatchPushEventDto matchPushEvent, int delay = 2000);

        Task SyncLiveMatches();

        Task SendMatchTimelines(MatchTimelineDto matchTimeline);

        Task SendLineups(LineUpsDto lineUpsDto);

        Task SendH2H(H2HDto h2HDto);
    }

    public class SimulatorService : ISimulatorService
    {
        private readonly string DirectoryRoot = AppDomain.CurrentDomain.BaseDirectory + "/App_Data";
        private readonly IConfiguration configuration;
        private readonly IMatchFileService matchFileService;

        private readonly ILogger logger;

        public SimulatorService(IConfiguration config, IMatchFileService matchFileService, ILogger logger)
        {
            configuration = config;
            this.matchFileService = matchFileService;
            this.logger = logger;
        }

        public async Task SendCleanExistData()
        {
            using (var triggerClient = new HttpClient
            {
                BaseAddress = new Uri(configuration.GetValue<string>("Simulator:Url"))
            })
            {
                var request = new HttpRequestMessage(HttpMethod.Post, configuration.GetValue<string>("Simulator:CleanMatch"));

                await triggerClient.SendAsync(request);
            }
        }

        public async Task SendCleanTimelines()
        {
            using (var triggerClient = new HttpClient
            {
                BaseAddress = new Uri(configuration.GetValue<string>("Simulator:Url"))
            })
            {
                var request = new HttpRequestMessage(HttpMethod.Post, configuration.GetValue<string>("Simulator:CleanTimelines"));

                await triggerClient.SendAsync(request);
            }
        }

        public async Task<bool> SendScheduleOdds(OddsScheduleDto oddsSchedule)
        {
            using (var httpClient = new HttpClient())
            {
                httpClient.BaseAddress = new Uri(configuration.GetValue<string>("Simulator:Url"));
                var requestPath = configuration.GetValue<string>("Simulator:ScheduleOdds");

                var responseMessage = await httpClient.PostAsJsonAsync(requestPath, oddsSchedule);

                if (responseMessage.IsSuccessStatusCode)
                {
                    logger.Info("Success send schedule odds", oddsSchedule);
                    return true;
                }
                else
                {
                    logger.Error(JsonConvert.SerializeObject(oddsSchedule), "Send schedule odds");
                }
            }

            return false;
        }

        public async Task<HttpResponseMessage> SendDailyResult(DailyResultDto dailyResultDto)
        {
            var jsonValue = JsonConvert.SerializeObject(dailyResultDto);
            var requestPath = configuration.GetValue<string>("Simulator:DailyResults");
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(configuration.GetValue<string>("Simulator:Url"));

                return await client.PostAsync(
                        string.Format(requestPath + "?date={0}",
                        dailyResultDto.generated_at.ToString("yyyy-MM-dd")),
                        new StringContent(jsonValue, Encoding.UTF8,
                        "application/json"));
            }
        }

        public async Task<HttpResponseMessage> SendDailySchedule(DailyScheduleDto dailyResultDto)
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(configuration.GetValue<string>("Simulator:Url"));

                var jsonValue = JsonConvert.SerializeObject(dailyResultDto);

                var requestPath = configuration.GetValue<string>("Simulator:DailySchedule");

                return await client.PostAsync(string.Format(requestPath + "?date={0}", dailyResultDto.generated_at.ToString("yyyy-MM-dd")), new StringContent(jsonValue, Encoding.UTF8, "application/json"));
            }
        }

        public async Task<HttpResponseMessage> SendLiveResult(DailyResultDto dailyResultDto)
        {
            var jsonValue = JsonConvert.SerializeObject(dailyResultDto);
            var requestPath = configuration.GetValue<string>("Simulator:LiveResults");
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(configuration.GetValue<string>("Simulator:Url"));

                return await client.PostAsync(
                        string.Format(requestPath + "?date={0}",
                        dailyResultDto.generated_at.ToString("yyyy-MM-dd")),
                        new StringContent(jsonValue, Encoding.UTF8,
                        "application/json"));
            }
        }

        public async Task SendMatchEvent(MatchPushEventDto matchPushEvent, int delay = 2000)
        {
            var matchRepository = new MatchRepository();
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(configuration.GetValue<string>("Simulator:Url"));
                var response = await client.PostAsJsonAsync(configuration.GetValue<string>("Simulator:PushEvent"), matchPushEvent);
                if (!response.IsSuccessStatusCode)
                {
                    return;
                }
            }
            await matchRepository.InsertTimelineLog(matchPushEvent);
            await Task.Delay(delay);
        }

        public async Task SyncLiveMatches()
        {
            var liveMatches = await GetLiveResult();

            matchFileService.SaveLiveMatchToFile(liveMatches);
        }

        public async Task SendMatchTimelines(MatchTimelineDto matchTimeline)
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(configuration.GetValue<string>("Simulator:Url"));
                var requestPath = configuration.GetValue<string>("Simulator:Timelines") + "?matchId=" +
                                  matchTimeline.sport_event.id;
                await client.PostAsJsonAsync(requestPath, matchTimeline);
            }
        }

        public async Task SendLineups(LineUpsDto lineUpsDto)
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(configuration.GetValue<string>("Simulator:Url"));
                var requestPath = configuration.GetValue<string>("Simulator:Lineups");

                await client.PostAsJsonAsync(requestPath, lineUpsDto);
            }
        }

        public async Task SendH2H(H2HDto h2HDto)
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(configuration.GetValue<string>("Simulator:Url"));
                var requestPath = configuration.GetValue<string>("Simulator:H2H");

                await client.PostAsJsonAsync(requestPath, h2HDto);
            }
        }

        public async Task<DailyResultDto> GetDailyResult(DateTime resultDate)
        {
            var resultDateName = resultDate.ToString("yyyy-MM-dd");
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(configuration.GetValue<string>("Simulator:Url"));
                var response = await client.GetAsync($"/api/soccer-t3/other/en-US/schedules/{resultDateName}/results.json");
                if (!response.IsSuccessStatusCode)
                {
                    return new DailyResultDto();
                }

                var responseBody = await response.Content.ReadAsStringAsync();
                var resultMatches = JsonConvert.DeserializeObject<DailyResultDto>(responseBody);

                return resultMatches;
            }
        }

        public async Task<DailyScheduleDto> GetDailySchedule(DateTime scheduleDate)
        {
            var scheduleDateName = scheduleDate.ToString("yyyy-MM-dd");
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(configuration.GetValue<string>("Simulator:Url"));
                var response = await client.GetAsync($"/api/soccer-t3/other/en-US/schedules/{scheduleDateName}/schedule.json");
                if (!response.IsSuccessStatusCode)
                {
                    return new DailyScheduleDto();
                }

                var responseBody = await response.Content.ReadAsStringAsync();
                var scheduleMatches = JsonConvert.DeserializeObject<DailyScheduleDto>(responseBody);

                return scheduleMatches;
            }
        }

        public async Task<DailyResultDto> GetLiveResult()
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(configuration.GetValue<string>("Simulator:Url"));
                var response = await client.GetAsync(configuration.GetValue<string>("Simulator:LiveMatches"));
                if (!response.IsSuccessStatusCode)
                {
                    return new DailyResultDto();
                }

                var responseBody = await response.Content.ReadAsStringAsync();
                var liveMatches = JsonConvert.DeserializeObject<DailyResultDto>(responseBody);

                return liveMatches;
            }
        }
    }
}
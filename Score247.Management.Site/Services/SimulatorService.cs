using Score247Management.Core.Scenarios.Models;

namespace Score247.Management.Site.Services
{
    using System;
    using System.IO;
    using System.Net.Http;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Configuration;
    using Newtonsoft.Json;
    using Models.Events;
    using Score247.Management.Site.Repositories.Matches;
    using Score247Management.Core.Odds.Dtos;

    public interface ISimulatorService
    {
        Task SendCleanExistData();

        Task SendCleanTimelines();

        Task<bool> SendScheduleOdds(OddsScheduleDto oddsSchedule);

        Task<HttpResponseMessage> SendDailyResult(DailyResultDto dailyResultDto);

        Task<HttpResponseMessage> SendLiveResult(DailyResultDto dailyResultDto);

        Task<HttpResponseMessage> SendDailySchedule(DailyScheduleDto dailyResultDto);

        Task SendMatchEvent(MatchPushEventDto matchPushEvent, int delay = 2000);

        Task SyncLiveMatches();

        Task SendMatchTimelines(MatchTimelineDto matchTimeline);
    }

    public class SimulatorService : ISimulatorService
    {
        private readonly string DirectoryRoot = AppDomain.CurrentDomain.BaseDirectory + "/App_Data";
        private IConfiguration configuration { get; }

        public SimulatorService(IConfiguration config)
        {
            configuration = config;
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

            await SendCacheLiveMatches();
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
                    return true;
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

        private async Task SendCacheLiveMatches()
        {
            var liveCached = await ReadLiveMatchFromFile();
            if (liveCached == null)
            {
                return;
            }

            await SendLiveResult(liveCached);
        }

        private async Task<DailyResultDto> ReadLiveMatchFromFile()
        {
            try
            {
                var resultFolder = $"{DirectoryRoot}";
                System.IO.Directory.CreateDirectory(resultFolder);

                var path = $"{resultFolder}/live.json";
                var jsonData = await File.ReadAllTextAsync(path);
                var content = JsonConvert.DeserializeObject<DailyResultDto>(jsonData);

                return content;
            }
            catch (Exception)
            {
                return null;
            }
        }

        private async Task WriteLiveMatchToFile(DailyResultDto resultDto)
        {
            try
            {
                var resultFolder = $"{DirectoryRoot}";
                System.IO.Directory.CreateDirectory(resultFolder);
                var path = $"{resultFolder}/live.json";
                var json = JsonConvert.SerializeObject(resultDto);

                System.IO.Directory.CreateDirectory(resultFolder);
                System.IO.File.Delete(path);
                System.IO.File.WriteAllText(path, json);
            }
            catch (Exception)
            {
            }
        }

        public async Task SyncLiveMatches()
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(configuration.GetValue<string>("Simulator:Url"));
                var response = await client.GetAsync(configuration.GetValue<string>("Simulator:LiveMatches"));
                if (!response.IsSuccessStatusCode)
                {
                    return;
                }

                var responseBody = await response.Content.ReadAsStringAsync();
                var liveMatches = JsonConvert.DeserializeObject<DailyResultDto>(responseBody);
                await WriteLiveMatchToFile(liveMatches);
            }
        }

        public async Task SendMatchTimelines(MatchTimelineDto matchTimeline)
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(configuration.GetValue<string>("Simulator:Url"));
                var requestPath = configuration.GetValue<string>("Simulator:Timelines") + "?matchId=" +
                                  matchTimeline.sport_event.id;
                var response = await client.PostAsJsonAsync(requestPath, matchTimeline);
                if (!response.IsSuccessStatusCode)
                {
                }
            }
        }
    }
}
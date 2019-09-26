namespace Score247.Management.Site.Services
{
    using System;
    using System.Net.Http;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Configuration;
    using Newtonsoft.Json;
    using Score247.Management.Site.Repositories.Matches.Models;
    using Score247Management.Core.Odds.Dtos;

    public interface ISimulatorService
    {
        Task SendCleanExistData();

        Task<bool> SendScheduleOdds(OddsScheduleDto oddsSchedule);

        Task<HttpResponseMessage> SendDailyResult(DailyResultDto dailyResultDto);

        Task<HttpResponseMessage> SendLiveResult(DailyResultDto dailyResultDto);

        Task<HttpResponseMessage> SendDailySchedule(DailyScheduleDto dailyResultDto);
    }

    public class SimulatorService : ISimulatorService
    {
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

                using (HttpResponseMessage httpResponseMessage = await triggerClient.SendAsync(request))
                {
                    _ = httpResponseMessage;
                }
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
    }
}
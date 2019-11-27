namespace Score247.Management.Site.Services
{
    using Fanex.Logging;
    using System;
    using System.Net.Http;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Configuration;
    using Newtonsoft.Json;
    using Score247.Management.Site.Models.Events;
    using Score247.Management.Site.Repositories.Matches;
    using Score247Management.Core.Odds.Dtos;
    using Score247.Management.Site.Models;
    using System.Linq;
    using System.Collections.Generic;
    using Score247.Management.Site.Models.Dtos;

    public interface ISimulatorService
    {
        Task SendCleanExistData();

        Task<bool> SendScheduleOdds(OddsScheduleDto oddsSchedule);

        Task<HttpResponseMessage> SendDailyResult(DailyResultDto dailyResultDto);

        Task<HttpResponseMessage> SendLiveResult(DailyResultDto dailyResultDto);

        Task<HttpResponseMessage> SendDailySchedule(DailyScheduleDto dailyResultDto);

        Task<DailyResultDto> GetDailyResult(DateTime resultDate);

        Task<DailyScheduleDto> GetDailySchedule(DateTime scheduleDate);

        Task<DailyResultDto> GetLiveResult();

        Task<LineUpsDto> GetLineUpsDto(string matchId);

        Task SendMatchEvent(MatchPushEventDto matchPushEvent, bool eventMissed = false, int delay = 2000);

        Task SyncLiveMatches();

        Task SendMatchTimelines(MatchTimelineDto matchTimeline);

        Task SendTeamResult(TeamResultDto teamResultDto);

        Task SendLineups(LineUpsDto lineUpsDto);

        Task SendH2H(H2HDto h2HDto);

        Task SendTournamentStanding(TournamentStandingDto tournamentStandingDto);

        Task SendTournamentSchedule(TournamentScheduleDto schedule);
    }

    public class SimulatorService : ISimulatorService
    {
        private readonly IConfiguration configuration;
        private readonly ISimulatorFileService matchFileService;

        private readonly ILogger logger;

        public SimulatorService(IConfiguration config, ISimulatorFileService matchFileService, ILogger logger)
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
                var request = new HttpRequestMessage(HttpMethod.Post, configuration.GetValue<string>("Simulator:CleanMatches"));

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
            var result = await ProcessMergeResult(dailyResultDto);
            var jsonValue = JsonConvert.SerializeObject(result);
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

        public async Task<HttpResponseMessage> SendDailySchedule(DailyScheduleDto dailyScheduleDto)
        {
            var schedule = await ProcessMergeSchedule(dailyScheduleDto);
            var jsonValue = JsonConvert.SerializeObject(schedule);
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(configuration.GetValue<string>("Simulator:Url"));

                var requestPath = configuration.GetValue<string>("Simulator:DailySchedule");

                return await client.PostAsync(string.Format(requestPath + "?date={0}", dailyScheduleDto.generated_at.ToString("yyyy-MM-dd")), new StringContent(jsonValue, Encoding.UTF8, "application/json"));
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

        public async Task SendMatchEvent(MatchPushEventDto matchPushEvent, bool eventMissed = false, int delay = 2000)
        {
            var matchRepository = new MatchRepository();
            await matchRepository.InsertTimelineLog(matchPushEvent);
            if (!eventMissed)
            {
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(configuration.GetValue<string>("Simulator:Url"));
                    var response = await client.PostAsJsonAsync(configuration.GetValue<string>("Simulator:PushEvent"), matchPushEvent);
                    if (!response.IsSuccessStatusCode)
                    {
                        return;
                    }
                }
            }

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

        public async Task SendTeamResult(TeamResultDto teamResultDto)
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(configuration.GetValue<string>("Simulator:Url"));
                var requestPath = configuration.GetValue<string>("Simulator:TeamResult");

                await client.PostAsJsonAsync(requestPath, teamResultDto);
            }
        }

        public async Task SendTournamentStanding(TournamentStandingDto tournamentStandingDto)
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(configuration.GetValue<string>("Simulator:Url"));
                var requestPath = configuration.GetValue<string>("Simulator:TournamentStandings");

                await client.PostAsJsonAsync(requestPath, tournamentStandingDto);
            }
        }

        public async Task<DailyResultDto> GetDailyResult(DateTime resultDate)
        {
            var resultDateName = resultDate.ToString("yyyy-MM-dd");
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(configuration.GetValue<string>("Simulator:Url"));
                var response = await client.GetAsync($"api/soccer-t3/other/en-US/schedules/{resultDateName}/results.json");
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
                var response = await client.GetAsync($"api/soccer-t3/other/en-US/schedules/{scheduleDateName}/schedule.json");
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

        private async Task<DailyResultDto> ProcessMergeResult(DailyResultDto newResult)
        {
            var simulatorResult = await GetDailyResult(newResult.generated_at);
            var results = simulatorResult.results?.ToList();
            if (results == null)
            {
                results = new List<Result>();
            }
            foreach (var result in newResult.results)
            {
                if (results.FirstOrDefault(x => x.sport_event.id == result.sport_event.id) == null)
                {
                    results.Add(result);
                }
            }
            simulatorResult.results = results;
            return simulatorResult;
        }

        private async Task<DailyScheduleDto> ProcessMergeSchedule(DailyScheduleDto newSchedule)
        {
            var simulatorSchedule = await GetDailySchedule(newSchedule.generated_at);
            var matches = simulatorSchedule.sport_events?.ToList();
            if (matches == null)
            {
                matches = new List<Sport_Event>();
            }
            foreach (var match in newSchedule.sport_events)
            {
                if (matches.FirstOrDefault(x => x.id == match.id) == null)
                {
                    matches.Add(match);
                }
            }
            simulatorSchedule.sport_events = matches;
            return simulatorSchedule;
        }

        public async Task<LineUpsDto> GetLineUpsDto(string matchId)
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(configuration.GetValue<string>("Simulator:Url"));
                var response = await client.GetAsync($"api/soccer-t3/other/en-US/matches/{matchId}/lineups.json");
                if (!response.IsSuccessStatusCode)
                {
                    return new LineUpsDto();
                }

                var responseBody = await response.Content.ReadAsStringAsync();
                var lineupsDto = JsonConvert.DeserializeObject<LineUpsDto>(responseBody);

                return lineupsDto;
            }
        }

        public async Task SendTournamentSchedule(TournamentScheduleDto schedule)
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(configuration.GetValue<string>("Simulator:Url"));
                var requestPath = configuration.GetValue<string>("Simulator:TournamentSchedule");

                await client.PostAsJsonAsync(requestPath, schedule);
            }
        }
    }
}
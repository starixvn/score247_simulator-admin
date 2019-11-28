using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Score247.Management.Site.Models;
using Score247.Management.Site.Models.Dtos;
using Score247.Management.Site.Models.Events;

namespace Score247.Management.Site.Services
{
    public interface ISimulatorFileService
    {
        Task<DailyResultDto> ReadLiveMatchFromFile();

        Task<H2HDto> ReadH2HFromFile(string h2hName);

        void SaveLiveMatchToFile(DailyResultDto content);

        Task UpdateSchedule(DailyScheduleDto result);

        Task<DailyScheduleDto> ReadSchedule(DateTime scheduleDate);

        Task UpdateResult(DailyResultDto result);

        Task<DailyResultDto> ReadResult(DateTime scheduleDate);

        Task<TeamResultDto> ReadTeamResult(string teamId);

        Task<TournamentStandingDto> ReadDefaultStanding();

        Task<IEnumerable<MatchTimelineDto>> GetAllTimelimeSample();

        Task<MatchTimelineDto> GetRandomTimelime();

        Task<TournamentScheduleDto> GetTournamentSchedule();

        Task<MatchTimelineDto> GetNotStartTimelime();
    }

    public class SimulatorFileService : ISimulatorFileService
    {
        private static readonly string DirectoryRoot = AppDomain.CurrentDomain.BaseDirectory + "App_Data";

        public async Task<IEnumerable<MatchTimelineDto>> GetAllTimelimeSample()
        {
            var path = $"{DirectoryRoot}\\timelines";
            var timelines = new List<MatchTimelineDto>();
            var fileTimelines = Directory.GetFiles(path);
            foreach (var timelineFile in fileTimelines)
            {
                var jsonData = await File.ReadAllTextAsync(timelineFile);
                var content = JsonConvert.DeserializeObject<MatchTimelineDto>(jsonData);
                timelines.Add(content);
            }

            return timelines;
        }

        public async Task<MatchTimelineDto> GetNotStartTimelime()
        {
            var path = $"{DirectoryRoot}\\timeline-notstart.json";
            var jsonData = await File.ReadAllTextAsync(path);

            return JsonConvert.DeserializeObject<MatchTimelineDto>(jsonData);
        }

        public async Task<MatchTimelineDto> GetRandomTimelime()
        {
            var rand = new Random();
            var timelines = (await GetAllTimelimeSample()).ToList();

            return timelines[rand.Next(timelines.Count)];
        }

        public async Task<TournamentScheduleDto> GetTournamentSchedule()
        {
            var path = $"{DirectoryRoot}/fixture/schedule.json";
            var jsonData = await File.ReadAllTextAsync(path);
            var content = JsonConvert.DeserializeObject<TournamentScheduleDto>(jsonData);

            return content;
        }

        public async Task<TournamentStandingDto> ReadDefaultStanding()
        {
            try
            {
                var path = $"{DirectoryRoot}/standing/default.json";

                var jsonData = await File.ReadAllTextAsync(path);
                var content = JsonConvert.DeserializeObject<TournamentStandingDto>(jsonData);

                return content;
            }
            catch
            {
                return null;
            }
        }

        public async Task<H2HDto> ReadH2HFromFile(string h2hName)
        {
            try
            {
                var resultFolder = $"{DirectoryRoot}/h2h";
                Directory.CreateDirectory(resultFolder);

                var path = $"{resultFolder}/{h2hName}.json";
                var jsonData = await File.ReadAllTextAsync(path);
                var content = JsonConvert.DeserializeObject<H2HDto>(jsonData);

                return content;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public async Task<DailyResultDto> ReadLiveMatchFromFile()
        {
            try
            {
                var resultFolder = $"{DirectoryRoot}";
                Directory.CreateDirectory(resultFolder);

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

        public async Task<DailyResultDto> ReadResult(DateTime scheduleDate)
        {
            try
            {
                var resultFolder = $"{DirectoryRoot}";
                Directory.CreateDirectory(resultFolder);
                var resultDateFileName = scheduleDate.ToString("yyyy-MM-dd");
                var path = $"{resultFolder}/result/{resultDateFileName}.json";
                var jsonData = await File.ReadAllTextAsync(path);
                var content = JsonConvert.DeserializeObject<DailyResultDto>(jsonData);

                return content;
            }
            catch (Exception)
            {
                return new DailyResultDto();
            }
        }

        public async Task<DailyScheduleDto> ReadSchedule(DateTime scheduleDate)
        {
            try
            {
                var resultFolder = $"{DirectoryRoot}";
                Directory.CreateDirectory(resultFolder);
                var scheduleDateFilename = scheduleDate.ToString("yyyy-MM-dd");
                var path = $"{resultFolder}/schedule/{scheduleDateFilename}.json";
                var jsonData = await File.ReadAllTextAsync(path);
                var content = JsonConvert.DeserializeObject<DailyScheduleDto>(jsonData);

                return content;
            }
            catch (Exception)
            {
                return new DailyScheduleDto();
            }
        }

        public async Task<TeamResultDto> ReadTeamResult(string teamId)
        {
            try
            {
                var resultFolder = $"{DirectoryRoot}/team-result";
                Directory.CreateDirectory(resultFolder);

                var path = $"{resultFolder}/{teamId}.json";
                var jsonData = await File.ReadAllTextAsync(path);
                var content = JsonConvert.DeserializeObject<TeamResultDto>(jsonData);

                return content;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public void SaveLiveMatchToFile(DailyResultDto content)
        {
            var resultFolder = $"{DirectoryRoot}";
            Directory.CreateDirectory(resultFolder);
            var path = $"{resultFolder}/live.json";
            var json = JsonConvert.SerializeObject(content);

            Directory.CreateDirectory(resultFolder);
            File.Delete(path);
            File.WriteAllText(path, json);
        }

        public async Task UpdateResult(DailyResultDto result)
        {
            var resultFolder = $"{DirectoryRoot}";
            Directory.CreateDirectory(resultFolder);
            var resultDateFileName = result.generated_at.ToString("yyyy-MM-dd");
            var path = $"{resultFolder}/result/{resultDateFileName}.json";
            var json = JsonConvert.SerializeObject(result);

            Directory.CreateDirectory(resultFolder);
            File.Delete(path);
            File.WriteAllText(path, json);
        }

        public async Task UpdateSchedule(DailyScheduleDto schedule)
        {
            var resultFolder = $"{DirectoryRoot}";
            Directory.CreateDirectory(resultFolder);
            var resultDateFileName = schedule.generated_at.ToString("yyyy-MM-dd");
            var path = $"{resultFolder}/result/{resultDateFileName}.json";
            var json = JsonConvert.SerializeObject(schedule);

            Directory.CreateDirectory(resultFolder);
            File.Delete(path);
            File.WriteAllText(path, json);
        }
    }
}
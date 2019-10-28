using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Score247.Management.Site.Models;
using Score247.Management.Site.Models.Events;

namespace Score247.Management.Site.Services
{
    public interface IMatchFileService
    {
        Task<DailyResultDto> ReadLiveMatchFromFile();

        Task<H2HDto> ReadH2HFromFile(string h2hName);

        void SaveLiveMatchToFile(DailyResultDto content);

        Task UpdateSchedule(Result result);

        Task ReadSchedule(DateTime scheduleDate);
    }

    public class MatchFileService : IMatchFileService
    {
        private static readonly string DirectoryRoot = AppDomain.CurrentDomain.BaseDirectory + "/App_Data";

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

        public async Task ReadSchedule(DateTime scheduleDate)
        {
            throw new NotImplementedException();
        }

        public Task UpdateSchedule(Result result)
        {
            throw new NotImplementedException();
        }
    }
}
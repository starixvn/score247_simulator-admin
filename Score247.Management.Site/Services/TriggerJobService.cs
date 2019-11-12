using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace Score247.Management.Site.Services
{
    public interface ITriggerJobService
    {
        Task TriggerJobs(IEnumerable<string> jobsName);

        Task TriggerFetchPreMatchJobs();

        Task TriggerFetchPostMatchJobs();

        Task TriggerCreateMatchJobs();

        Task TriggerFetchOddsJobs();

        Task TriggerFetchOddsChangeLogsJobs();

        Task TriggerFetchLiveMatch();

        Task TriggerFetchLiveMatchTimeline();

        Task TriggerFetchCleanMajorLeagueCache();

        Task TriggerFetchMatchLineups();
    }

    public class TriggerJobService : ITriggerJobService
    {
        private IConfiguration configuration { get; }

        public TriggerJobService(IConfiguration config)
        {
            configuration = config;
        }

        public async Task TriggerJobs(IEnumerable<string> jobsName)
        {
            using (var triggerClient = new HttpClient
            {
                BaseAddress = new Uri(configuration.GetValue<string>("Simulator:FetchUrl"))
            })
            {
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "");

                var content = new List<KeyValuePair<string, string>>();
                foreach (var jobName in jobsName)
                {
                    content.Add(new KeyValuePair<string, string>("jobs[]", jobName));
                }

                request.Content = new FormUrlEncodedContent(content);
                try
                {
                    using (var httpResponseMessage = await triggerClient.SendAsync(request))
                    {
                        _ = httpResponseMessage;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw;
                }
            }
        }

        public async Task TriggerCreateMatchJobs()
        {
            var jobs = new List<string> { "FetchPreMatches", "FetchPostMatches" };

            await TriggerJobs(jobs);
        }

        public async Task TriggerFetchOddsJobs()
        {
            var jobs = new List<string> { "FetchOdds" };

            await TriggerJobs(jobs);
        }

        public async Task TriggerFetchOddsChangeLogsJobs()
        {
            var jobs = new List<string> { "FetchOddsChangeLogs" };

            await TriggerJobs(jobs);
        }

        public async Task TriggerFetchLiveMatch()
        {
            var jobs = new List<string> { "FetchLiveMatches" };

            await TriggerJobs(jobs);
        }

        public async Task TriggerFetchLiveMatchTimeline()
        {
            var jobs = new List<string> { "FetchLiveMatchesTimeline" };

            await TriggerJobs(jobs);
        }

        public async Task TriggerFetchCleanMajorLeagueCache()
        {
            var jobs = new List<string> { "CleanMajorLeaguesCache" };

            await TriggerJobs(jobs);
        }

        public async Task TriggerFetchPostMatchJobs()
        {
            var jobs = new List<string> { "FetchPostMatches" };

            await TriggerJobs(jobs);
        }

        public async Task TriggerFetchMatchLineups()
        {
            var jobs = new List<string> { "FetchMatchLineups" };

            await TriggerJobs(jobs);
        }

        public async Task TriggerFetchPreMatchJobs()
        {
            var jobs = new List<string> { "FetchPreMatches" };

            await TriggerJobs(jobs);
        }
    }
}
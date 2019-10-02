using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace Score247.Management.Site.Services
{
    public interface ITriggerJobService
    {
        Task TriggerJobs(IEnumerable<string> jobsName);

        Task TriggerCreateMatchJobs();

        Task TriggerFetchOddsJobs();

        Task TriggerFetchOddsChangeLogsJobs();

        Task TriggerFetchLiveMatch();

        Task TriggerFetchLiveMatchTimeline();
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
            using (HttpClient triggerClient = new HttpClient
            {
                BaseAddress = new Uri(configuration.GetValue<string>("Simulator:FetchUrl"))
            })
            {
                var request = new HttpRequestMessage(HttpMethod.Post, "");

                var content = new List<KeyValuePair<string, string>>();
                foreach (var jobName in jobsName)
                {
                    content.Add(new KeyValuePair<string, string>("jobs[]", jobName));
                }

                request.Content = new FormUrlEncodedContent(content);

                using (HttpResponseMessage httpResponseMessage = await triggerClient.SendAsync(request))
                {
                    _ = httpResponseMessage;
                }
            }
        }

        public async Task TriggerCreateMatchJobs()
        {
            var jobs = new List<string>() { "FetchPreMatch", "FetchPostMatch" };

            await TriggerJobs(jobs);
        }

        public async Task TriggerFetchOddsJobs()
        {
            var jobs = new List<string>() { "FetchOdds" };

            await TriggerJobs(jobs);
        }

        public async Task TriggerFetchOddsChangeLogsJobs()
        {
            var jobs = new List<string>() { "FetchOddsChangeLogs" };

            await TriggerJobs(jobs);
        }

        public async Task TriggerFetchLiveMatch()
        {
            var jobs = new List<string>() { "FetchLiveMatch" };

            await TriggerJobs(jobs);
        }

        public async Task TriggerFetchLiveMatchTimeline()
        {
            var jobs = new List<string>() { "FetchLiveMatchTimeline" };

            await TriggerJobs(jobs);
        }
    }
}
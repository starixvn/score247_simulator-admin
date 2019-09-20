using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace Score247.Management.Site.Services
{
    public static class TriggerSimulatorService
    {
        public static async Task SendCleanExistData(IConfiguration configuration)
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

        public static async Task StartInsertMatchesJobs(IConfiguration configuration)
        {
            using (HttpClient triggerClient = new HttpClient
            {
                BaseAddress = new Uri(configuration.GetValue<string>("Simulator:FetchUrl"))
            })
            {
                var request = new HttpRequestMessage(HttpMethod.Post, "");

                var fetchContent = new FormUrlEncodedContent(new[]{
                            new KeyValuePair < string, string > ("jobs[]", "FetchPreMatch"),
                            new KeyValuePair < string, string > ("jobs[]", "FetchPostMatch"),
                            new KeyValuePair < string, string > ("jobs[]", "FetchLiveMatch")});

                request.Content = fetchContent;

                using (HttpResponseMessage httpResponseMessage = await triggerClient.SendAsync(request))
                {
                    _ = httpResponseMessage;
                }
            }
        }
    }
}
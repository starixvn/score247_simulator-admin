namespace Score247.Management.Site.Services.Crawlers
{
    using System;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Newtonsoft.Json;
    using Score247Management.Core.Images;

    public static class CountryFlagCrawlerService
    {
        private static readonly string APIKEY = "ktubexzckfjqxtupp68pcwju";
        private static readonly string RequestFlagPrefix = "https://api.sportradar.us/flags-images-p3/sr";
        private static readonly string RequestFlagPath = "/flags-images-p3/sr/country-flags/flags/manifest.json?api_key=";

        public static async Task<ImageManifest> GetCountryImageManifest()
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(RequestFlagPrefix);
                var response = await client.GetAsync(RequestFlagPath + APIKEY);
                if (!response.IsSuccessStatusCode)
                {
                    return null;
                }
                var responseBody = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<ImageManifest>(responseBody);
            }
        }
    }
}
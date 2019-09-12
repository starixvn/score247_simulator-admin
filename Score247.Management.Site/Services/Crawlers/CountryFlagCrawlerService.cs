namespace Score247.Management.Site.Services.Crawlers
{
    using System;
    using System.IO;
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Newtonsoft.Json;
    using Score247.Management.Site.Criterias.Images;
    using Score247Management.Core.Images;
    using System.Linq;
    using Score247.Management.Site.Helpers;
    using Score247.Management.Site.Repositories.Images;

    public static class CountryFlagCrawlerService
    {
        private static readonly string APIKEY = "ktubexzckfjqxtupp68pcwju";
        private static readonly string RequestFlagPrefix = "https://api.sportradar.us";
        private static readonly string APIPATH = "/flags-images-p3/sr";
        private static readonly string RequestFlagPath = "/country-flags/flags/manifest.json?api_key=";

        public static async Task<ImageManifest> GetCountryImageManifest()
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(RequestFlagPrefix);
                var response = await client.GetAsync(APIPATH + RequestFlagPath + APIKEY);
                if (!response.IsSuccessStatusCode)
                {
                    return null;
                }
                var responseBody = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<ImageManifest>(responseBody);
            }
        }

        public static async Task<byte[]> GetCountryImageBlob(string url)
        {
            HttpWebRequest myRequest = (HttpWebRequest)WebRequest.Create(url);
            myRequest.Method = "GET";
            WebResponse myResponse = await myRequest.GetResponseAsync();
            MemoryStream ms = new MemoryStream();
            myResponse.GetResponseStream().CopyTo(ms);
            byte[] data = ms.ToArray();

            return data;
        }

        public static async Task GetAndInsertImageToDataBase()
        {
            var manifest = await CountryFlagCrawlerService.GetCountryImageManifest();
            var imageRepo = new ImageRepository();
            foreach (var asset in manifest.assetlist)
            {
                foreach (var image in asset.links)
                {
                    try
                    {
                        var criteria = new InsertImageCriteria();
                        criteria.ShortName = asset.refs.FirstOrDefault().iso_country_code;
                        criteria.Name = asset.title;
                        var url = RequestFlagPrefix + APIPATH + image.href + "?api_key=" + APIKEY;
                        var blob = await GetCountryImageBlob(url);
                        criteria.Href = url;
                        criteria.ImageData = blob;
                        criteria.Width = image.width;
                        criteria.Height = image.height;
                        criteria.ImageType = "country";
                        criteria.Id = UniqueGenerator.GetIntId();

                        await imageRepo.InsertImage(criteria);
                    }
                    catch (Exception)
                    {
                    }
                }
            }
        }
    }
}
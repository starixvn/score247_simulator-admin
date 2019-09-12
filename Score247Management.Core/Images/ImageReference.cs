using Newtonsoft.Json;

namespace Score247Management.Core.Images
{
    public class ImageReference
    {
        public string name { get; set; }
        public string type { get; set; }
        public string sport { get; set; }
        public string sportradar_id { get; set; }

        [JsonProperty(PropertyName = "iso_country_code=")]
        public string iso_country_code { get; set; }

        public bool primary { get; set; }
    }
}
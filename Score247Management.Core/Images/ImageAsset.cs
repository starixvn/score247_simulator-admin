using System;
using System.Collections.Generic;

namespace Score247Management.Core.Images
{
    public class ImageAsset
    {
        public Guid id { get; set; }
        public DateTime created { get; set; }
        public DateTime updated { get; set; }
        public string title { get; set; }
        public string description { get; set; }
        public string team_id { get; set; }
        public string copyright { get; set; }
        public string type { get; set; }
        public IEnumerable<ImageLink> links { get; set; }
    }
}
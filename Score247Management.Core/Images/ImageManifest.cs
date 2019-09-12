using System;
using System.Collections.Generic;

namespace Score247Management.Core.Images
{
    public class ImageManifest
    {
        public string provider { get; set; }
        public string league { get; set; }
        public string type { get; set; }
        public DateTime manifest_date { get; set; }
        public bool trial { get; set; }
        public IEnumerable<ImageAsset> assetlist { get; set; }
    }
}
using System;
using System.IO;

namespace Score247.Management.Site.Helpers
{
    public static class ImageHelper
    {
        public static string ConvertImageToStringData(byte[] source)
        {
            string imageBase64Data = Convert.ToBase64String(source);
            string imageDataURL = string.Format("data:image/svg+xml;base64,{0}", imageBase64Data);

            return imageDataURL;
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fanex.Data.Repository;

namespace Score247.Management.Site.Criterias.Images
{
    public class InsertImageCriteria : NonQueryCommand
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string ShortName { get; set; }
        public string Href { get; set; }
        public string ImageType { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }

        public byte[] ImageData { get; set; }

        public override string GetSettingKey()
            => "Image_Insert";

        public override bool IsValid()
            => true;
    }
}
namespace Score247Management.Core.Images
{
    public class ImageSource
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public string ShortName { get; set; }
        public string Href { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public bool IsOriginal { get; set; }
        public byte[] ImageData { get; set; }
    }
}
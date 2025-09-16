namespace AurelsOpenAIClient.Images.Response
{
    public class CreateImageResponse
    {
        public long created { get; set; }
        public string background { get; set; }
        public ImageData[] data { get; set; }
        public string output_format { get; set; }
        public string quality { get; set; }
        public string size { get; set; }
        public Usage usage { get; set; }
    }
}

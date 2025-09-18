namespace AurelsOpenAIClient.Images.Request
{
    /// <summary>
    /// Request body for the CreateImage endpoint.
    /// </summary>
    public class CreateImageRequest
    {
        public string model { get; set; }
        public string prompt { get; set; }
        public int n { get; set; }
        public string size { get; set; }
        public string background { get; set; }
        public string quality { get; set; }
        public string output_format { get; set; }
        public int? output_compression { get; set; }
        public int? partial_images { get; set; }

        public CreateImageRequest(string model,
            string prompt,
            string size = "1024x1024",
            int n = 1,
            string background = "auto",
            string output_format = "png",
            string quality = "auto",
            int? output_compression = 100,
            int? partial_images = 0
            )
        {
            this.model = model;
            this.prompt = prompt;
            this.n = n;
            this.size = size;
            this.background = background;
            this.output_format = output_format;
            this.quality = quality;
            this.output_compression = output_compression;
            this.partial_images = partial_images;
        }
    }
}
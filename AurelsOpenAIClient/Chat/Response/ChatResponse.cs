namespace AurelsOpenAIClient.Chat.Response
{
    public class ChatResponse
    {
        public Choice[] choices { get; set; }
        public long created { get; set; }
        public string id { get; set; }
        public string model { get; set; }
        public string @object { get; set; }
        public Usage usage { get; set; }
    }
}

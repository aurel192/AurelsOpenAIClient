namespace AurelsOpenAIClient.Chat.Response
{
    public class Message
    {
        public string content { get; set; }
        public string role { get; set; }
        public string refusal { get; set; } = null;
        public string[] annotations { get; set; } = null;
    }
}

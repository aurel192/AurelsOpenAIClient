namespace AurelsOpenAIClient.Chat.Response
{
    public class Choice
    {
        public string finish_reason { get; set; }
        public int index { get; set; }
        public Message message { get; set; }
        public object logprobs { get; set; }
        public Delta delta { get; set; }
    }
}

namespace AurelsOpenAIClient.Chat.Parameters
{
    public class ChatCompletionsMessage
    {
        public string role { get; set; }

        public string content { get; set; }

        public ChatCompletionsMessage(string role, string content)
        {
            this.role = role;
            this.content = content;
        }

        public override string ToString()
        {
            return $"role:{role} content:{content}";
        }
    }
}

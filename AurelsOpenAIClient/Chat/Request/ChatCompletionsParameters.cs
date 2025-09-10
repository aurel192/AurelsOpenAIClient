using System.Collections.Generic;

namespace AurelsOpenAIClient.Chat.Parameters
{
    public class ChatCompletionsParameters
    {
        public List<ChatCompletionsMessage> messages { get; set; } = new List<ChatCompletionsMessage>();
        public string model { get; set; }
        public bool stream { get; set; } = false;
        public int max_tokens { get; set; }
        public float temperature { get; set; }
        public float top_p { get; set; }
        public float frequency_penalty { get; set; }
        public float presence_penalty { get; set; }

        public ChatCompletionsParameters(string model, List<ChatCompletionsMessage> messages, int max_tokens = 5000, float temperature = 0.7f, float top_p = 1, float frequency_penalty = 0, float presence_penalty = 0)
        {
            this.model = model;
            this.max_tokens = max_tokens;
            this.temperature = temperature;
            this.top_p = top_p;
            this.frequency_penalty = frequency_penalty;
            this.presence_penalty = presence_penalty;
            this.messages = messages;
        }
    }
}

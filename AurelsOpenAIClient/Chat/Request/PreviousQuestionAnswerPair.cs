using AurelsOpenAIClient.Chat.Parameters;
using System;

namespace AurelsOpenAIClient.Chat.Request
{
    public class PreviousQuestionAnswerPair
    {
        public DateTime Timestamp { get; set; }

        public ChatCompletionsMessage Question { get; set; }

        public ChatCompletionsMessage Answer { get; set; }

        public PreviousQuestionAnswerPair(string question, string answer)
        {
            this.Timestamp = DateTime.Now;
            this.Question = new ChatCompletionsMessage(role: "user", content: question);
            this.Answer = new ChatCompletionsMessage(role: "assistant", content: answer);
        }

        public override string ToString()
        {
            return $"Timestamp:{Timestamp.ToShortTimeString()} Question:{Question.ToString()} Answer:{Answer.ToString()}";
        }
    }
}

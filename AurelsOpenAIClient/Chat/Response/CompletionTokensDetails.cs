namespace AurelsOpenAIClient.Chat.Response
{
    public class CompletionTokensDetails
    {
        public int reasoning_tokens { get; set; }
        public int audio_tokens { get; set; }
        public int accepted_prediction_tokens { get; set; }
        public int rejected_prediction_tokens { get; set; }
    }
}

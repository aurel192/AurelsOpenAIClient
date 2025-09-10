namespace AurelsOpenAIClient.Audio.Request
{
    public class SpeechGenerationRequest
    {
        public string model { get; set; }
        public string input { get; set; }
        public string voice { get; set; }
        public float speed { get; set; }

        public SpeechGenerationRequest(string Input, string Model, Voices Voice, float Speed)
        {
            this.input = Input;
            this.model = Model;
            this.voice = Voice.ToString();
            this.speed = Speed;
        }

        public SpeechGenerationRequest(string Input, string Model, string Voice, float Speed)
        {
            this.input = Input;
            this.model = Model;
            this.voice = Voice;
            this.speed = Speed;
        }
    }
}

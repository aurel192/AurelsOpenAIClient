namespace AurelsOpenAIClient.Audio.Response
{
    public class Usage
    {
        public string type { get; set; }
        public int total_tokens { get; set; }
        public int input_tokens { get; set; }
        public InputTokenDetails input_token_details { get; set; }
        public int output_tokens { get; set; }
        public float seconds { get; set; }
    }
}

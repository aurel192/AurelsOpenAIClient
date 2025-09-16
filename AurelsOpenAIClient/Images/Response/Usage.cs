namespace AurelsOpenAIClient.Images.Response
{
    public class Usage
    {
        public int input_tokens { get; set; }
        public InputTokensDetails input_tokens_details { get; set; }
        public int output_tokens { get; set; }
        public int total_tokens { get; set; }
    }
}

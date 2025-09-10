using System.Collections.Generic;

namespace AurelsOpenAIClient.Audio.Response
{
    public class TranscriptionResponse
    {
        public string text { get; set; }
        public string task { get; set; }
        public string language { get; set; }
        public float duration { get; set; }
        public Usage usage { get; set; }
        public List<Segment> segments { get; set; }
    }
}

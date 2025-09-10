using System.Collections.Generic;

namespace AurelsOpenAIClient.Audio.Response
{
    public class Segment
    {
        public int id { get; set; }
        public int seek { get; set; }
        public double start { get; set; }
        public double end { get; set; }
        public string text { get; set; }
        public List<int> tokens { get; set; }
        public double temperature { get; set; }
        public double avg_logprob { get; set; }
        public double compression_ratio { get; set; }
        public double no_speech_prob { get; set; }
    }
}

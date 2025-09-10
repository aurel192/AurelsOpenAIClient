using System;
using System.Net.Http;

namespace AurelsOpenAIClient
{
    public class OpenAiCommonBase
    {
        internal string endpoint { get; set; }
        internal string model { get; set; } 
        internal int maxTokens { get; set; } = 100000;
        internal HttpClient httpClient { get; set; }
        internal string jsonRequest { get; set; } = string.Empty;
        internal string jsonResponse { get; set; } = string.Empty;
        internal int responseTime { get; set; } = 0;

        /// <summary>
        /// Set the model to be used for the request (e.g. "gpt-4", "gpt-3.5-turbo")
        /// </summary>
        /// <param name="model"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public void SetModel(string model)
        {
            if (string.IsNullOrEmpty(model))
                throw new ArgumentNullException("Model can not be empty.");

            this.model = model;
        }

        /// <summary>
        /// Set the API endpoint for the request (e.g. "https://api.openai.com/v1/chat/completions")
        /// </summary>
        /// <param name="endpoint"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public void SetEndpoint(string endpoint)
        {
            if (string.IsNullOrEmpty(endpoint))
                throw new ArgumentNullException("Endpoint can not be empty.");

            this.endpoint = endpoint;
        }

        /// <summary>
        /// You can set the maximum number of tokens for the response (default: 5000)
        /// </summary>
        /// <param name="role"></param>
        /// <exception cref="ArgumentException"></exception>
        public void SetMaxTokens(int maxTokens)
        {
            if (maxTokens <= 0)
                throw new ArgumentException("Max tokens must be a positive integer!");

            this.maxTokens = maxTokens;
        }

        public int GetResponseTimeMs() => this.responseTime;

        public string GetJsonRequest() => this.jsonRequest;

        public string GetJsonResponse() => this.jsonResponse;

    }
}

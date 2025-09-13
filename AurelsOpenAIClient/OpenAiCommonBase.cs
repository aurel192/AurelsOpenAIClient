using System;
using System.Net.Http;
using System.Text.Json;

namespace AurelsOpenAIClient
{
    public abstract class OpenAiCommonBase : IDisposable
    {
        protected readonly HttpClient _httpClient;
        protected string _endpoint;
        protected string _model;
        protected int _maxTokens;
        protected string _jsonRequest;
        protected string _jsonResponse;
        protected int _responseTime;

        protected OpenAiCommonBase(string apiKey, string organization, string project)
        {
            _httpClient = new HttpClient();
            if (!string.IsNullOrEmpty(apiKey))
                _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
            else
                throw new ApplicationException("API key is not set.");

            if (!string.IsNullOrEmpty(organization))
                _httpClient.DefaultRequestHeaders.Add("OpenAI-Organization", organization);

            if (!string.IsNullOrEmpty(project))
                _httpClient.DefaultRequestHeaders.Add("OpenAI-Project", project);
        }

        /// <summary>
        /// Set the model to be used for the request (e.g. "gpt-4o", "gpt-3.5-turbo")
        /// </summary>
        /// <param name="model"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public void SetModel(string model)
        {
            if (string.IsNullOrEmpty(model))
                throw new ArgumentNullException("Model can not be empty.");

            _model = model;
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

            _endpoint = endpoint;
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

            _maxTokens = maxTokens;
        }

        public int GetResponseTimeMs() => _responseTime;

        public string GetJsonRequest()
        {
            if (string.IsNullOrEmpty(_jsonRequest))
                return string.Empty;

            using (JsonDocument doc = JsonDocument.Parse(_jsonRequest))
            {
                return JsonSerializer.Serialize(doc.RootElement, new JsonSerializerOptions { WriteIndented = true });
            }
        }

        public string GetJsonResponse()
        {
            if (string.IsNullOrEmpty(_jsonResponse))
                return string.Empty;

            using (JsonDocument doc = JsonDocument.Parse(_jsonResponse))
            {
                return JsonSerializer.Serialize(doc.RootElement, new JsonSerializerOptions { WriteIndented = true });
            }
        }

        public void Dispose()
        {
            _httpClient.Dispose();
        }
    }
}

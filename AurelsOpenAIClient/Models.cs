using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace AurelsOpenAIClient
{
    /// <summary>
    /// This GET endpoint is used to list all the available models by OpenAI
    /// https://platform.openai.com/docs/models
    /// </summary>
    /// <param name="apiKey"></param>
    /// <param name="organization">Optional</param>
    /// <param name="project">Optional</param>
    /// <exception cref="Exception"></exception>
    public class Models
    {
        internal HttpClient httpClient { get; set; }

        public Models(string apiKey)
        {
            httpClient = new HttpClient();

            if (!string.IsNullOrEmpty(apiKey))
                httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
            else
                throw new ApplicationException("API key is not set.");
        }

        public async Task<string> GetModels()
        {
            string endpoint = "https://api.openai.com/v1/models";
            HttpResponseMessage response = await httpClient.GetAsync(endpoint);
            if (!response.IsSuccessStatusCode)
            {
                string error = await response.Content.ReadAsStringAsync();
                throw new ApplicationException($"Error retrieving models: {response.StatusCode} - {error}");
            }
            string jsonResponse = await response.Content.ReadAsStringAsync();
            string jsonFormatted = Newtonsoft.Json.Linq.JToken.Parse(jsonResponse).ToString(Newtonsoft.Json.Formatting.Indented);
            return jsonFormatted;
        }
    }
}

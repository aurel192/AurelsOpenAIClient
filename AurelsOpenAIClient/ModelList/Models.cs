using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace AurelsOpenAIClient.ModelList
{
    public class Models : OpenAiCommonBase
    {
        /// <summary>
        /// This GET endpoint is used to list all the available models by OpenAI
        /// https://platform.openai.com/docs/models
        /// </summary>
        /// <param name="apiKey">Get an OpenAI API key! (It is not the ChatGPT Pricing plan.</param>
        /// <param name="organization">Optional</param>
        /// <param name="project">Optional</param>
        /// <exception cref="Exception"></exception>
        public Models(string apiKey, string organization = null, string project = null) : base(apiKey, organization, project)
        {
        }

        public async Task<string> GetModels()
        {
            string endpoint = "https://api.openai.com/v1/models";
            HttpResponseMessage response = await _httpClient.GetAsync(endpoint);
            if (!response.IsSuccessStatusCode)
            {
                string error = await response.Content.ReadAsStringAsync();
                throw new ApplicationException($"Error retrieving models: {response.StatusCode} - {error}");
            }
            string jsonResponse = await response.Content.ReadAsStringAsync();
            string jsonFormatted;
            using (JsonDocument doc = JsonDocument.Parse(jsonResponse))
            {
                jsonFormatted = JsonSerializer.Serialize(doc.RootElement, new JsonSerializerOptions { WriteIndented = true });
            }
            return jsonFormatted;
        }
    }
}

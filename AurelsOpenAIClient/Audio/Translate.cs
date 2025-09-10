using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Newtonsoft.Json;
using AurelsOpenAIClient.Audio.Response;

namespace AurelsOpenAIClient.Audio
{
    /// <summary>
    /// Translate audio into English.
    /// Available model: whisper-1
    /// https://platform.openai.com/docs/api-reference/audio/createTranslation
    /// </summary>
    /// <param name="apiKey"></param>
    /// <param name="organization">Optional</param>
    /// <param name="project">Optional</param>
    /// <exception cref="Exception"></exception>
    public class Translate : OpenAiCommonBase
    {
        public Translate(string apiKey, string organization = null, string project = null)
        {
            this.httpClient = new HttpClient();
            this.endpoint = "https://api.openai.com/v1/audio/translations";
            this.model = "whisper-1"; // only whisper-1 is available at 2025-09-03

            if (!string.IsNullOrEmpty(apiKey))
                this.httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
            else
                throw new ApplicationException("API key is not set.");

            if (!string.IsNullOrEmpty(organization))
                this.httpClient.DefaultRequestHeaders.Add("OpenAI-Organization", organization);

            if (!string.IsNullOrEmpty(project))
                this.httpClient.DefaultRequestHeaders.Add("OpenAI-Project", project);
        }

        public async Task<string> GetResponse(string input = "InputVoice.mp3", float temperature = 0.0f)
        {
            if (string.IsNullOrEmpty(input))
                throw new IOException("Input file is not set.");

            if (!File.Exists(input))
                throw new FileNotFoundException($"{input} File does not exists.");

            if (string.IsNullOrEmpty(this.model))
                throw new ApplicationException("LLM AI Model is not set.");

            if (temperature < 0 || temperature > 1)
                throw new ApplicationException("Speed must be a value between 0.0 and 1.0");

            string translatedResponse = string.Empty;

            try
            {
                MultipartFormDataContent formData = new MultipartFormDataContent();
                ByteArrayContent audioFileContent = new ByteArrayContent(File.ReadAllBytes(input));
                audioFileContent.Headers.ContentType = new MediaTypeHeaderValue("audio/mpeg");

                formData.Add(audioFileContent, "file", Path.GetFileName(input));
                formData.Add(new StringContent(this.model), "model");

                DateTime start = DateTime.Now;

                HttpResponseMessage response = await httpClient.PostAsync(endpoint, formData);
                response.EnsureSuccessStatusCode();

                responseTime = (int)(DateTime.Now - start).TotalMilliseconds;

                string responseBody = await response.Content.ReadAsStringAsync();
                TranslateResponse responseText = JsonConvert.DeserializeObject<TranslateResponse>(responseBody);

                translatedResponse = responseText?.text?.ToString();
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return translatedResponse;
        }

    }
}

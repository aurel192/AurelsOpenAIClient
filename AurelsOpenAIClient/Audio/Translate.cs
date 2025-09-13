using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Text.Json;
using AurelsOpenAIClient.Audio.Response;

namespace AurelsOpenAIClient.Audio
{
    public class Translate : OpenAiCommonBase
    {
        /// <summary>
        /// Translate audio into English.
        /// Available model: whisper-1
        /// https://platform.openai.com/docs/api-reference/audio/createTranslation
        /// </summary>
        /// <param name="apiKey"></param>
        /// <param name="organization">Optional</param>
        /// <param name="project">Optional</param>
        /// <exception cref="ApplicationException"></exception>
        public Translate(string apiKey, string organization = null, string project = null) : base(apiKey, organization, project)
        {
            _endpoint = "https://api.openai.com/v1/audio/translations";
            _model = "whisper-1"; // only whisper-1 is available at 2025-09-03
        }

        public async Task<string> GetResponse(string input = "InputVoice.mp3", float temperature = 0.0f)
        {
            if (string.IsNullOrEmpty(input))
                throw new IOException("Input file is not set.");

            if (!File.Exists(input))
                throw new FileNotFoundException($"{input} File does not exists.");

            if (string.IsNullOrEmpty(_model))
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
                formData.Add(new StringContent(_model), "model");

                DateTime start = DateTime.Now;

                HttpResponseMessage response = await _httpClient.PostAsync(_endpoint, formData);
                response.EnsureSuccessStatusCode();

                _responseTime = (int)(DateTime.Now - start).TotalMilliseconds;

                string responseBody = await response.Content.ReadAsStringAsync();

                // Use System.Text.Json instead of Newtonsoft.Json
                JsonSerializerOptions options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };
                TranslateResponse responseText = JsonSerializer.Deserialize<TranslateResponse>(responseBody, options);

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

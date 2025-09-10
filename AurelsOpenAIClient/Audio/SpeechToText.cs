using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using AurelsOpenAIClient.Audio.Response;

namespace AurelsOpenAIClient.Chat
{
    /// <summary>
    /// Transcribe models: whisper-1, gpt-4o-transcribe, gpt-4o-mini-transcribe
    /// https://platform.openai.com/docs/api-reference/audio/createTranscription
    /// </summary>
    /// <param name="apiKey"></param>
    /// <param name="organization"></param>
    /// <param name="project"></param>
    /// <exception cref="Exception"></exception>
    public class SpeechToText : OpenAiCommonBase
    {
        private float temperature = 0.0f;
        private TranscriptionResponse transcriptionResponse;

        public SpeechToText(string apiKey, string organization = null, string project = null)
        {
            this.httpClient = new HttpClient();
            this.endpoint = "https://api.openai.com/v1/audio/transcriptions";
            this.model = "gpt-4o-transcribe"; // default model
            this.temperature = 0.0f; // default temperature

            if (!string.IsNullOrEmpty(apiKey))
                this.httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
            else
                throw new ApplicationException("API key is not set.");

            if (!string.IsNullOrEmpty(organization))
                this.httpClient.DefaultRequestHeaders.Add("OpenAI-Organization", organization);

            if (!string.IsNullOrEmpty(project))
                this.httpClient.DefaultRequestHeaders.Add("OpenAI-Project", project);
        }

        /// <summary>
        /// Transcribes audio into the input language.
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="language">Optinal. The language of the input audio. Supplying the input language in ISO-639-1 (e.g. en) format will improve accuracy and latency.</param>
        /// <param name="response_format">Optional. Defaults is json. The format of the output, in one of these options: json, text, srt, verbose_json, or vtt.For gpt-4o-transcribe and gpt-4o-mini-transcribe, the only supported format is json</param>
        /// <param name="promt">Optional. The prompt can be used to provide context to the model about the audio content. It can be useful for improving accuracy in specific scenarios, such as when there is background noise or when the audio contains specialized terminology.</param>
        /// <param name="chunking_strategy">Optional when set to auto, the server first normalizes loudness and then uses voice activity detection (VAD) to choose boundaries. If unset, the audio is transcribed as a single block. Thus if you want to stream audio transcription chunks, you have to set it to auto when setting stream = True.</param>
        /// <returns>transcribed string</returns>
        /// <exception cref="ApplicationException"></exception>
        public async Task<string> Transcribe(string filePath, string language = null, string response_format = null, string promt = null, object chunking_strategy = null)
        {
            if (string.IsNullOrEmpty(filePath))
                throw new ApplicationException("filePath is not set.");

            if (!File.Exists(filePath))
                throw new ApplicationException($"filePath:{filePath} not exists.");

            string transcribedResponse = string.Empty;

            try
            {
                MultipartFormDataContent formData = new MultipartFormDataContent();
                ByteArrayContent audioFileContent = new ByteArrayContent(File.ReadAllBytes(filePath));
                audioFileContent.Headers.ContentType = new MediaTypeHeaderValue("audio/mpeg");

                formData.Add(audioFileContent, "file", Path.GetFileName(filePath));
                formData.Add(new StringContent(this.model), "model");
                formData.Add(new StringContent(this.temperature.ToString()), "temperature");

                if (!(chunking_strategy is null))
                {
                    if (chunking_strategy is string)
                    {
                        formData.Add(new StringContent(chunking_strategy.ToString()), "chunking_strategy");
                    }
                    else
                    {
                        string chunking_strategy_json = JsonConvert.SerializeObject(chunking_strategy);
                        formData.Add(new StringContent(chunking_strategy_json), "chunking_strategy");
                    }
                }

                // Optionally: if you know the language, you can set it: "en", "es", etc...
                if (!string.IsNullOrEmpty(language))
                    formData.Add(new StringContent(language), "language");

                // Optionally: json, text, srt, verbose_json, or vtt.
                if (!string.IsNullOrEmpty(response_format))
                    formData.Add(new StringContent(response_format), "response_format");

                // Optionally: if you know the language, you can set it: "en", "es", etc...
                if (!string.IsNullOrEmpty(promt))
                    formData.Add(new StringContent(promt), "promt");

                DateTime start = DateTime.Now;

                HttpResponseMessage response = await httpClient.PostAsync(endpoint, formData);
                response.EnsureSuccessStatusCode();

                responseTime = (int)(DateTime.Now - start).TotalMilliseconds;

                string responseBody = await response.Content.ReadAsStringAsync();
                transcriptionResponse = JsonConvert.DeserializeObject<TranscriptionResponse>(responseBody);

                transcribedResponse = transcriptionResponse?.text?.ToString();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }

            return transcribedResponse;
        }

        /// <summary>
        /// Optional parameter.
        ///  Defaults value is 0
        /// The sampling temperature, between 0 and 1.
        /// Higher values like 0.8 will make the output more random, while lower values like 0.2 will make it more focused and deterministic.
        /// If set to 0, the model will use log probability to automatically increase the temperature until certain thresholds are hit.
        /// </summary>
        /// <param name="role"></param>
        /// <exception cref="ArgumentException"></exception>
        public void SetTemperature(float temperature)
        {
            if (temperature < 0 || temperature > 1)
                throw new ApplicationException("Temperature must be a value between 0.0 and 1.0");

            this.temperature = temperature;
        }

        public TranscriptionResponse GetFullTranscriptionResponse() => transcriptionResponse;
    }
}

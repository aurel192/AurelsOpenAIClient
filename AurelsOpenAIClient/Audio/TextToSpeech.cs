using AurelsOpenAIClient.Audio.Request;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace AurelsOpenAIClient.Chat
{
    public class TextToSpeech : OpenAiCommonBase
    {
        /// <summary>
        /// TTS models: tts-1, tts-1-hd or gpt-4o-mini-tts
        /// https://platform.openai.com/docs/api-reference/audio/createSpeech
        /// </summary>
        /// <param name="apiKey"></param>
        /// <param name="organization">Optional</param>
        /// <param name="project">Optional</param>
        /// <exception cref="Exception"></exception>
        public TextToSpeech(string apiKey, string organization = null, string project = null)
        {
            this.httpClient = new HttpClient();
            this.endpoint = "https://api.openai.com/v1/audio/speech";
            this.model = "tts-1"; // default model

            if (!string.IsNullOrEmpty(apiKey))
                this.httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
            else
                throw new ApplicationException("OpenAI API key is not set.");

            if (!string.IsNullOrEmpty(organization))
                this.httpClient.DefaultRequestHeaders.Add("OpenAI-Organization", organization);

            if (!string.IsNullOrEmpty(project))
                this.httpClient.DefaultRequestHeaders.Add("OpenAI-Project", project);
        }

        /// <summary>
        /// Enum Voice: alloy, ash, ballad, coral, echo, fable, onyx, nova, sage, shimmer, verse
        /// </summary>
        /// <param name="text"></param>
        /// <param name="voice"></param>
        /// <param name="speed"></param>
        /// <param name="pathToSaveMP3"></param>
        /// <returns>string responseFilePath</returns>
        /// <exception cref="Exception"></exception>
        public async Task<string> GetResponse(string text, Voices voice = Voices.alloy, float speed = 1f, string pathToSaveMP3 = "GeneratedSpeech.mp3")
        {
            CheckParameters(text, speed);

            return await GetResponseFromAPI(text, voice.ToString(), speed, pathToSaveMP3);
        }

        /// <summary>
        /// string Voice: alloy, ash, ballad, coral, echo, fable, onyx, nova, sage, shimmer, verse
        /// </summary>
        /// <param name="text"></param>
        /// <param name="voice"></param>
        /// <param name="speed"></param>
        /// <param name="pathToSaveMP3"></param>
        /// <returns>string responseFileName</returns>
        /// <exception cref="Exception"></exception>
        public async Task<string> GetResponse(string text, string voice = "alloy", float speed = 1f, string pathToSaveMP3 = "GeneratedSpeech.mp3")
        {
            CheckParameters(text, speed);

            return await GetResponseFromAPI(text, voice, speed, pathToSaveMP3);
        }

        private void CheckParameters(string text, float speed)
        {
            if (string.IsNullOrEmpty(text))
                throw new ApplicationException("OpenAI: text is not set.");

            if (text.Length > 4096)
                throw new ApplicationException("text must be shorter than 4096 characters.");

            if (string.IsNullOrEmpty(model))
                throw new ApplicationException("OpenAI Model is not set.");

            if (speed < 0.25 || speed > 4)
                throw new ApplicationException("Speed must be a value between 0.25 and 4.0");
        }

        private async Task<string> GetResponseFromAPI(string text, string voice, float speed, string pathToSaveMP3)
        {
            try
            {
                // Request JSON 
                SpeechGenerationRequest speechGenerationRequest = new SpeechGenerationRequest(Input: text, Model: model, Voice: voice, Speed: speed);

                jsonRequest = JsonConvert.SerializeObject(speechGenerationRequest);

                StringContent jsonContent = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

                DateTime start = DateTime.Now;

                HttpResponseMessage response = await httpClient.PostAsync(endpoint, jsonContent);
                response.EnsureSuccessStatusCode();

                responseTime = (int)(DateTime.Now - start).TotalMilliseconds;

                byte[] mp3Data = await response.Content.ReadAsByteArrayAsync();

                if (mp3Data == null || mp3Data.Length == 0)
                    throw new ApplicationException("Response voice MP3 data is empty!");

                string responseFileName = await WriteMP3File(pathToSaveMP3, mp3Data);
                return responseFileName;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private async Task<string> WriteMP3File(string pathToSaveMP3, byte[] mp3Data)
        {
            File.WriteAllBytes(pathToSaveMP3, mp3Data);

            int cntr = 0;
            while (true)
            {
                await Task.Delay(30);
                if (cntr++ > 500)
                    throw new IOException($"Cannot write file: {pathToSaveMP3}");

                if (File.Exists(pathToSaveMP3))
                {
                    return Path.GetFileName(pathToSaveMP3);
                }
            }
        }

    }
}

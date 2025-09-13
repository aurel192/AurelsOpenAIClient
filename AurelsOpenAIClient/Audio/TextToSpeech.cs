using AurelsOpenAIClient.Audio.Request;
using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace AurelsOpenAIClient.Chat
{
    /// <summary>
    /// TTS models: tts-1, tts-1-hd or gpt-4o-mini-tts
    /// https://platform.openai.com/docs/api-reference/audio/createSpeech
    /// </summary>
    /// <param name="apiKey"></param>
    /// <param name="organization">Optional</param>
    /// <param name="project">Optional</param>
    /// <exception cref="Exception"></exception>
    public class TextToSpeech : OpenAiCommonBase
    {
        public TextToSpeech(string apiKey, string organization = null, string project = null) : base(apiKey, organization, project)
        {
            _endpoint = "https://api.openai.com/v1/audio/speech";
            _model = "tts-1"; // default model
        }

        /// <summary>
        /// Generate speech audio (MP3) from text input.
        /// </summary>
        /// <param name="text">The text to be converted to audio</param>
        /// <param name="voice">Voice(Enum): alloy, ash, ballad, coral, echo, fable, onyx, nova, sage, shimmer, verse</param>
        /// <param name="speed">Number between 0.25 and 4.0</param>
        /// <param name="pathToSaveMP3"></param>
        /// <returns>string responseFilePath</returns>
        /// <exception cref="Exception"></exception>
        public async Task<string> GetResponse(string text, Voices voice = Voices.alloy, float speed = 1f, string pathToSaveMP3 = "GeneratedSpeech.mp3")
        {
            CheckParameters(text, speed);

            return await GetResponseFromAPI(text, voice.ToString(), speed, pathToSaveMP3);
        }

        /// <summary>
        /// Generate speech audio (MP3) from text input.
        /// </summary>
        /// <param name="text">The text to be converted to audio</param>
        /// <param name="voice">Voices(string): alloy, ash, ballad, coral, echo, fable, onyx, nova, sage, shimmer, verse</param>
        /// <param name="speed">Number between 0.25 and 4.0</param>
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

            if (string.IsNullOrEmpty(_model))
                throw new ApplicationException("OpenAI Model is not set.");

            if (speed < 0.25 || speed > 4)
                throw new ApplicationException("Speed must be a value between 0.25 and 4.0");
        }

        private async Task<string> GetResponseFromAPI(string text, string voice, float speed, string pathToSaveMP3)
        {
            try
            {
                SpeechGenerationRequest speechGenerationRequest = new SpeechGenerationRequest(Input: text, Model: _model, Voice: voice, Speed: speed);

                _jsonRequest = JsonSerializer.Serialize(speechGenerationRequest, new JsonSerializerOptions { WriteIndented = true });

                StringContent jsonContent = new StringContent(_jsonRequest, Encoding.UTF8, "application/json");

                DateTime start = DateTime.Now;

                HttpResponseMessage response = await _httpClient.PostAsync(_endpoint, jsonContent);
                response.EnsureSuccessStatusCode();

                _responseTime = (int)(DateTime.Now - start).TotalMilliseconds;

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
                if (cntr++ > 1000) // wait max 30 seconds
                    throw new IOException($"Cannot write file: {pathToSaveMP3}");

                if (File.Exists(pathToSaveMP3))
                {
                    return Path.GetFileName(pathToSaveMP3);
                }
            }
        }

    }
}

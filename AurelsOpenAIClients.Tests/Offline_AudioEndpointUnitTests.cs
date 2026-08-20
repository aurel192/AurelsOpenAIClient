using System.Net.Http;
using System.Reflection;
using System.Text.Json;
using AurelsOpenAIClient.Audio;
using AurelsOpenAIClient.Audio.Request;
using AurelsOpenAIClient.Chat;

namespace AurelsOpenAIClient.Tests
{
    public class Offline_AudioEndpointUnitTests
    {
        [Fact]
        public async Task SpeechToText_RejectsMissingFileAndInvalidTemperature()
        {
            using SpeechToText client = new SpeechToText(LoadSettings.ReadPropertyFromJson("ApiKey"));

            await Assert.ThrowsAsync<ApplicationException>(() => client.Transcribe(null));
            await Assert.ThrowsAsync<ApplicationException>(() => client.Transcribe("missing-audio.mp3"));
            Assert.Throws<ApplicationException>(() => client.SetTemperature(-0.1f));
            Assert.Throws<ApplicationException>(() => client.SetTemperature(1.1f));
        }

        [Fact]
        public async Task SpeechToText_Transcribe_SendsMultipartRequestAndParsesResponse()
        {
            using SpeechToText client = new SpeechToText(LoadSettings.ReadPropertyFromJson("ApiKey"));
            TestHttpMessageHandler handler = new TestHttpMessageHandler(_ =>
                TestHttpMessageHandler.JsonResponse("{\"text\":\"hello transcription\",\"language\":\"en\"}"));
            ReplaceHttpClient(client, handler);
            string audioPath = GetSampleAudioFilePath();

            string result = await client.Transcribe(
                audioPath,
                language: "en",
                response_format: "json",
                prompt: "test prompt",
                chunking_strategy: new { type = "auto" });

            Assert.Equal("hello transcription", result);
            Assert.Equal("hello transcription", client.GetFullTranscriptionResponse().text);
            Assert.Single(handler.Requests);
            Assert.Equal("POST", handler.Requests[0].Method.Method);
            Assert.Contains("/v1/audio/transcriptions", handler.Requests[0].RequestUri.AbsolutePath);

            string body = await handler.Requests[0].Content.ReadAsStringAsync();
            Assert.Contains("model", body);
            Assert.Contains("language", body);
            Assert.Contains("response_format", body);
            Assert.Contains("prompt", body);
            Assert.Contains("chunking_strategy", body);
        }

        [Fact]
        public async Task TextToSpeech_RejectsInvalidParameters()
        {
            using TextToSpeech client = new TextToSpeech(LoadSettings.ReadPropertyFromJson("ApiKey"));

            await Assert.ThrowsAsync<ApplicationException>(() => client.GetResponse(string.Empty, (string)null));
            await Assert.ThrowsAsync<ApplicationException>(() => client.GetResponse(new string('x', 4097), (string)null));
            await Assert.ThrowsAsync<ApplicationException>(() => client.GetResponse("hello", (string)null, 0.2f));
            await Assert.ThrowsAsync<ApplicationException>(() => client.GetResponse("hello", (string)null, 4.1f));
        }

        [Fact]
        public async Task TextToSpeech_BothOverloads_SaveBinaryResponse()
        {
            using TextToSpeech client = new TextToSpeech(LoadSettings.ReadPropertyFromJson("ApiKey"));
            TestHttpMessageHandler handler = new TestHttpMessageHandler(_ =>
                TestHttpMessageHandler.BinaryResponse(new byte[] { 0x49, 0x44, 0x33, 0x01 }));
            ReplaceHttpClient(client, handler);
            string enumOutput = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".mp3");
            string stringOutput = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".mp3");

            try
            {
                string enumResult = await client.GetResponse("hello", Voices.nova, 1, enumOutput);
                string stringResult = await client.GetResponse("hello", "sage", 1, stringOutput);

                Assert.Equal(Path.GetFileName(enumOutput), enumResult);
                Assert.Equal(Path.GetFileName(stringOutput), stringResult);
                Assert.Equal(new byte[] { 0x49, 0x44, 0x33, 0x01 }, File.ReadAllBytes(enumOutput));
                Assert.Equal(new byte[] { 0x49, 0x44, 0x33, 0x01 }, File.ReadAllBytes(stringOutput));
                Assert.Equal(2, handler.Requests.Count);

                using JsonDocument firstRequest = JsonDocument.Parse(await handler.Requests[0].Content.ReadAsStringAsync());
                Assert.Equal("nova", firstRequest.RootElement.GetProperty("voice").GetString());
                using JsonDocument secondRequest = JsonDocument.Parse(await handler.Requests[1].Content.ReadAsStringAsync());
                Assert.Equal("sage", secondRequest.RootElement.GetProperty("voice").GetString());
            }
            finally
            {
                File.Delete(enumOutput);
                File.Delete(stringOutput);
            }
        }

        [Fact]
        public async Task Translate_RejectsInvalidInputAndTemperature()
        {
            using Translate client = new Translate(LoadSettings.ReadPropertyFromJson("ApiKey"));

            await Assert.ThrowsAsync<IOException>(() => client.GetResponse(string.Empty));
            await Assert.ThrowsAsync<FileNotFoundException>(() => client.GetResponse("missing-audio.mp3"));
            string sampleAudioPath = GetSampleAudioFilePath();
            await Assert.ThrowsAsync<ApplicationException>(() => client.GetResponse(sampleAudioPath, -0.1f));
            await Assert.ThrowsAsync<ApplicationException>(() => client.GetResponse(sampleAudioPath, 1.1f));
        }

        [Fact]
        public async Task Translate_GetResponse_SendsMultipartRequestAndParsesResponse()
        {
            using Translate client = new Translate(LoadSettings.ReadPropertyFromJson("ApiKey"));
            TestHttpMessageHandler handler = new TestHttpMessageHandler(_ =>
                TestHttpMessageHandler.JsonResponse("{\"text\":\"translated text\"}"));
            ReplaceHttpClient(client, handler);
            string audioPath = GetSampleAudioFilePath();

            string result = await client.GetResponse(audioPath);

            Assert.Equal("translated text", result);
            Assert.Single(handler.Requests);
            Assert.Contains("model", await handler.Requests[0].Content.ReadAsStringAsync());
        }

        private static string GetSampleAudioFilePath()
        {
            return Path.Combine(AppContext.BaseDirectory, "SampleAudioFile.mp3");
        }

        private static void ReplaceHttpClient(object client, HttpMessageHandler handler)
        {
            FieldInfo field = typeof(AurelsOpenAIClient.OpenAiCommonBase).GetField("_httpClient", BindingFlags.Instance | BindingFlags.NonPublic);
            field.SetValue(client, new HttpClient(handler));
        }
    }
}

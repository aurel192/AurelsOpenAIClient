using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Text.Json;
using AurelsOpenAIClient.Images;

namespace AurelsOpenAIClient.Tests
{
    public class Offline_ImageEndpointUnitTests
    {
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task Generate_RejectsMissingPrompt(string prompt)
        {
            using GenerateImage client = new GenerateImage(LoadSettings.ReadPropertyFromJson("ApiKey"));

            await Assert.ThrowsAsync<ApplicationException>(() => client.Generate(prompt));
        }

        [Fact]
        public async Task Generate_RejectsInvalidArgumentsAndExistingOutput()
        {
            using GenerateImage client = new GenerateImage(LoadSettings.ReadPropertyFromJson("ApiKey"));

            await Assert.ThrowsAsync<ApplicationException>(() => client.Generate("prompt", n: 0));
            await Assert.ThrowsAsync<ApplicationException>(() => client.Generate("prompt", size: string.Empty));
            await Assert.ThrowsAsync<ApplicationException>(() => client.Generate("prompt", outputFileName: string.Empty));

            string existingPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".png");
            File.WriteAllBytes(existingPath, new byte[] { 1 });
            try
            {
                await Assert.ThrowsAsync<ApplicationException>(() => client.Generate("prompt", existingPath));
            }
            finally
            {
                File.Delete(existingPath);
            }
        }

        [Fact]
        public async Task Generate_DecodesBase64ImageAndSavesFile()
        {
            using GenerateImage client = new GenerateImage(LoadSettings.ReadPropertyFromJson("ApiKey"));
            byte[] imageBytes = Encoding.UTF8.GetBytes("fake image bytes");
            TestHttpMessageHandler handler = new TestHttpMessageHandler(_ =>
                TestHttpMessageHandler.JsonResponse($"{{\"created\":1,\"data\":[{{\"b64_json\":\"{Convert.ToBase64String(imageBytes)}\"}}]}}"));
            ReplaceHttpClient(client, handler);
            string outputPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".png");

            try
            {
                string[] files = await client.Generate("a test image", outputPath);

                Assert.Equal(new[] { outputPath }, files);
                Assert.Equal(imageBytes, File.ReadAllBytes(outputPath));
                Assert.Single(handler.Requests);
                using JsonDocument request = JsonDocument.Parse(await handler.Requests[0].Content.ReadAsStringAsync());
                Assert.Equal("dall-e-3", request.RootElement.GetProperty("model").GetString());
                Assert.Equal("a test image", request.RootElement.GetProperty("prompt").GetString());
            }
            finally
            {
                File.Delete(outputPath);
            }
        }

        [Fact]
        public async Task Generate_WithMultipleImages_AddsSuffixes()
        {
            using GenerateImage client = new GenerateImage(LoadSettings.ReadPropertyFromJson("ApiKey"));
            byte[] imageBytes = new byte[] { 7, 8, 9 };
            string base64 = Convert.ToBase64String(imageBytes);
            ReplaceHttpClient(client, new TestHttpMessageHandler(_ =>
                TestHttpMessageHandler.JsonResponse($"{{\"data\":[{{\"b64_json\":\"{base64}\"}},{{\"b64_json\":\"{base64}\"}}]}}")));
            string outputPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".png");
            string firstPath = Path.Combine(Path.GetDirectoryName(outputPath), Path.GetFileNameWithoutExtension(outputPath) + "_1.png");
            string secondPath = Path.Combine(Path.GetDirectoryName(outputPath), Path.GetFileNameWithoutExtension(outputPath) + "_2.png");

            try
            {
                string[] files = await client.Generate("prompt", outputPath, n: 2);

                Assert.Equal(new[] { firstPath, secondPath }, files);
                Assert.Equal(imageBytes, File.ReadAllBytes(firstPath));
                Assert.Equal(imageBytes, File.ReadAllBytes(secondPath));
            }
            finally
            {
                File.Delete(firstPath);
                File.Delete(secondPath);
            }
        }

        [Fact]
        public async Task Generate_WhenResponseHasNoImageData_Throws()
        {
            using GenerateImage client = new GenerateImage(LoadSettings.ReadPropertyFromJson("ApiKey"));
            ReplaceHttpClient(client, new TestHttpMessageHandler(_ =>
                TestHttpMessageHandler.JsonResponse("{\"data\":[{}]}")));

            await Assert.ThrowsAsync<ApplicationException>(() => client.Generate(
                "prompt", Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".png")));
        }

        private static void ReplaceHttpClient(object client, HttpMessageHandler handler)
        {
            FieldInfo field = typeof(AurelsOpenAIClient.OpenAiCommonBase).GetField("_httpClient", BindingFlags.Instance | BindingFlags.NonPublic);
            field.SetValue(client, new HttpClient(handler));
        }
    }
}

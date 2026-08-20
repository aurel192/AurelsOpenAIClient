using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text.Json;
using AurelsOpenAIClient.ModelList;

namespace AurelsOpenAIClient.Tests
{
    public class Offline_ModelsUnitTests
    {
        [Fact]
        public async Task GetModels_FormatsSuccessfulResponse()
        {
            using Models client = new Models(LoadSettings.ReadPropertyFromJson("ApiKey"));
            TestHttpMessageHandler handler = new TestHttpMessageHandler(request =>
            {
                Assert.Equal(HttpMethod.Get, request.Method);
                Assert.Equal("https://api.openai.com/v1/models", request.RequestUri.ToString());
                return TestHttpMessageHandler.JsonResponse("{\"object\":\"list\",\"data\":[{\"id\":\"test-model\"}]}" );
            });
            ReplaceHttpClient(client, handler);

            string response = await client.GetModels();

            using JsonDocument document = JsonDocument.Parse(response);
            Assert.Equal("list", document.RootElement.GetProperty("object").GetString());
            Assert.Equal("test-model", document.RootElement.GetProperty("data")[0].GetProperty("id").GetString());
            Assert.Single(handler.Requests);
        }

        [Fact]
        public async Task GetModels_WhenServerFails_ThrowsApplicationExceptionWithDetails()
        {
            using Models client = new Models(LoadSettings.ReadPropertyFromJson("ApiKey"));
            ReplaceHttpClient(client, new TestHttpMessageHandler(_ =>
                TestHttpMessageHandler.JsonResponse("{\"error\":\"invalid\"}", HttpStatusCode.Unauthorized)));

            ApplicationException exception = await Assert.ThrowsAsync<ApplicationException>(() => client.GetModels());

            Assert.Contains("Unauthorized", exception.Message);
            Assert.Contains("invalid", exception.Message);
        }

        private static void ReplaceHttpClient(Models client, HttpMessageHandler handler)
        {
            FieldInfo field = typeof(AurelsOpenAIClient.OpenAiCommonBase).GetField("_httpClient", BindingFlags.Instance | BindingFlags.NonPublic);
            field.SetValue(client, new HttpClient(handler));
        }
    }
}

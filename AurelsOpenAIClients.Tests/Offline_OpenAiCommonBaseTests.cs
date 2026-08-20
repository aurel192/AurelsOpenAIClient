using System.Net.Http;
using System.Reflection;
using System.Text.Json;
using AurelsOpenAIClient.Chat;

namespace AurelsOpenAIClient.Tests
{
    public class Offline_OpenAiCommonBaseTests
    {
        [Fact]
        public void Constructor_RequiresApiKey()
        {
            Assert.Throws<ApplicationException>(() => new ChatCompletion(null));
            Assert.Throws<ApplicationException>(() => new ChatCompletion(string.Empty));
        }

        [Fact]
        public void Constructor_AddsAuthenticationHeader()
        {
            string apiKey = LoadSettings.ReadPropertyFromJson("ApiKey");
            using ChatCompletion client = new ChatCompletion(apiKey);
            HttpClient httpClient = GetHttpClient(client);

            Assert.Equal($"Bearer {apiKey}", httpClient.DefaultRequestHeaders.Authorization?.ToString());
        }

        [Fact]
        public void SetModel_AndSetEndpoint_RejectEmptyValues()
        {
            using ChatCompletion client = new ChatCompletion(LoadSettings.ReadPropertyFromJson("ApiKey"));

            Assert.Throws<ArgumentNullException>(() => client.SetModel(null));
            Assert.Throws<ArgumentNullException>(() => client.SetModel(string.Empty));
            Assert.Throws<ArgumentNullException>(() => client.SetEndpoint(null));
            Assert.Throws<ArgumentNullException>(() => client.SetEndpoint(string.Empty));

            client.SetModel("test-model");
            client.SetEndpoint("https://localhost/test");

            Assert.Equal("test-model", GetField<string>(client, "_model"));
            Assert.Equal("https://localhost/test", GetField<string>(client, "_endpoint"));
        }

        [Fact]
        public void SetMaxTokens_RejectsNonPositiveValues_AndAcceptsPositiveValues()
        {
            using ChatCompletion client = new ChatCompletion(LoadSettings.ReadPropertyFromJson("ApiKey"));

            Assert.Throws<ArgumentException>(() => client.SetMaxTokens(0));
            Assert.Throws<ArgumentException>(() => client.SetMaxTokens(-1));

            client.SetMaxTokens(123);

            Assert.Equal(123, GetField<int>(client, "_maxTokens"));
        }

        [Fact]
        public void JsonAndResponseTimeGetters_ReturnExpectedValues()
        {
            using ChatCompletion client = new ChatCompletion(LoadSettings.ReadPropertyFromJson("ApiKey"));

            Assert.Equal(string.Empty, client.GetJsonRequest());
            Assert.Equal(string.Empty, client.GetJsonResponse());
            Assert.Equal(0, client.GetResponseTimeMs());

            SetField(client, "_jsonRequest", "{\"b\":2,\"a\":1}");
            SetField(client, "_jsonResponse", "not-json");
            SetField(client, "_responseTime", 42);

            string formattedRequest = client.GetJsonRequest();
            Assert.Contains("\"b\": 2", formattedRequest);
            Assert.Contains("\"a\": 1", formattedRequest);
            Assert.Equal("not-json", client.GetJsonResponse());
            Assert.Equal(42, client.GetResponseTimeMs());
        }

        [Fact]
        public async Task Dispose_CanBeCalled()
        {
            ChatCompletion client = new ChatCompletion(LoadSettings.ReadPropertyFromJson("ApiKey"));

            client.Dispose();

            await Assert.ThrowsAsync<ObjectDisposedException>(() => GetHttpClient(client).GetAsync("https://localhost"));
        }

        private static HttpClient GetHttpClient(object instance)
        {
            return GetField<HttpClient>(instance, "_httpClient");
        }

        private static T GetField<T>(object instance, string fieldName)
        {
            FieldInfo field = FindField(instance.GetType(), fieldName);
            return (T)field.GetValue(instance);
        }

        private static void SetField<T>(object instance, string fieldName, T value)
        {
            FieldInfo field = FindField(instance.GetType(), fieldName);
            field.SetValue(instance, value);
        }

        private static FieldInfo FindField(Type type, string fieldName)
        {
            while (type != null)
            {
                FieldInfo field = type.GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
                if (field != null)
                    return field;

                type = type.BaseType;
            }

            throw new MissingFieldException(fieldName);
        }
    }
}

using System.Net.Http;
using System.Reflection;
using System.Text.Json;
using AurelsOpenAIClient.Chat;
using AurelsOpenAIClient.Chat.Parameters;
using AurelsOpenAIClient.Chat.Request;

namespace AurelsOpenAIClient.Tests
{
    public class Offline_ChatCompletionUnitTests
    {
        [Fact]
        public void SetSystemRole_RejectsEmptyValue_AndRestoresDefault()
        {
            using ChatCompletion client = new ChatCompletion(LoadSettings.ReadPropertyFromJson("ApiKey"));

            ArgumentException exception = Assert.Throws<ArgumentException>(() => client.SetSystemRole(string.Empty));

            Assert.Contains("SystemRole", exception.Message);
            Assert.Equal("You are a helpful assistant.", GetField<string>(client, "_systemRole"));
        }

        [Theory]
        [InlineData(-0.1f)]
        [InlineData(2.1f)]
        public void SetTemperature_RejectsValuesOutsideSupportedRange(float temperature)
        {
            using ChatCompletion client = new ChatCompletion(LoadSettings.ReadPropertyFromJson("ApiKey"));

            Assert.Throws<ApplicationException>(() => client.SetTemperature(temperature));
        }

        [Fact]
        public void SetTemperature_AcceptsBoundaryValues()
        {
            using ChatCompletion client = new ChatCompletion(LoadSettings.ReadPropertyFromJson("ApiKey"));

            client.SetTemperature(0);
            Assert.Equal(0, GetField<float>(client, "_temperature"));

            client.SetTemperature(2);
            Assert.Equal(2, GetField<float>(client, "_temperature"));
        }

        [Fact]
        public void ConversationSearchAndClear_WorkWithoutCallingApi()
        {
            using ChatCompletion client = new ChatCompletion(LoadSettings.ReadPropertyFromJson("ApiKey"));

            Assert.Empty(client.GetLastQuestionAnswerPairsContainingKeywords(null));
            Assert.Empty(client.GetLastQuestionAnswerPairsContainingKeywords(new List<string>()));
            Assert.Empty(client.GetMostRecentQuestionAnswerPairs(10));

            AddConversationPair(client, "first question", "first answer");
            AddConversationPair(client, "second question about weather", "second answer");

            List<PreviousQuestionAnswerPair> recent = client.GetMostRecentQuestionAnswerPairs(1);
            Assert.Single(recent);
            Assert.Equal("second question about weather", recent[0].Question.content);

            List<PreviousQuestionAnswerPair> matches = client.GetLastQuestionAnswerPairsContainingKeywords(new List<string> { "WEATHER" });
            Assert.Single(matches);
            Assert.Equal("second answer", matches[0].Answer.content);

            client.ClearPreviousQuestionAndAnswerPairs();
            Assert.Empty(client.GetMostRecentQuestionAnswerPairs(10));
        }

        [Fact]
        public async Task SendChat_AndResponseGetters_ParseResponseAndRecordConversation()
        {
            using ChatCompletion client = new ChatCompletion(LoadSettings.ReadPropertyFromJson("ApiKey"));
            TestHttpMessageHandler handler = new TestHttpMessageHandler(_ => TestHttpMessageHandler.JsonResponse(ChatResponseJson("Hello from test", 7, 3, 10)));
            ReplaceHttpClient(client, handler);
            client.SetModel("test-model");

            string response = await client.SendChat("Say hello", throwExceptionWhenErrorOccours: true);

            Assert.Equal("Hello from test", response);
            Assert.Equal("Hello from test", client.GetLastAnswer());
            Assert.Equal(10, client.GetTotalTokens());
            Assert.Equal(7, client.GetPromptTokens());
            Assert.Equal(3, client.GetCompletionTokens());
            Assert.NotNull(client.GetFullChatResponse());
            Assert.True(client.GetResponseTimeMs() >= 0);
            string request = await handler.Requests.Single().Content.ReadAsStringAsync();
            Assert.Contains("Say hello", request);
            Assert.Contains("Hello from test", client.GetJsonResponse());
            Assert.Single(client.GetMostRecentQuestionAnswerPairs(1));
            Assert.Equal("Say hello", client.GetMostRecentQuestionAnswerPairs(1)[0].Question.content);
            Assert.Equal("Hello from test", client.GetMostRecentQuestionAnswerPairs(1)[0].Answer.content);
            Assert.Equal("POST", handler.Requests.Single().Method.Method);
            Assert.Contains("/v1/chat/completions", handler.Requests.Single().RequestUri.AbsolutePath);
        }

        [Fact]
        public async Task SendChatOverloads_AndAdvancedRequest_AreSupported()
        {
            using ChatCompletion client = new ChatCompletion(LoadSettings.ReadPropertyFromJson("ApiKey"));
            int call = 0;
            TestHttpMessageHandler handler = new TestHttpMessageHandler(_ =>
                TestHttpMessageHandler.JsonResponse(ChatResponseJson($"response-{++call}", 1, 2, 3)));
            ReplaceHttpClient(client, handler);
            client.SetModel("test-model");

            Assert.Equal("response-1", await client.SendChat("first"));
            Assert.Equal("response-2", await client.SendChat("second", 1));
            Assert.Equal("response-3", await client.SendChat("third", new List<string> { "first" }));
            Assert.Equal("response-4", await client.SendChat(
                new List<ChatCompletionsMessage>
                {
                    new ChatCompletionsMessage("user", "fourth")
                }));
            Assert.Equal("response-5", await client.SendChatAdvanced(
                new ChatCompletionsParameters("test-model", new List<ChatCompletionsMessage>
                {
                    new ChatCompletionsMessage("user", "fifth")
                })));

            Assert.Equal(5, handler.Requests.Count);
            string request = await handler.Requests[1].Content.ReadAsStringAsync();
            using JsonDocument document = JsonDocument.Parse(request);
            Assert.Contains("first", document.RootElement.GetProperty("messages").ToString());
        }

        [Fact]
        public async Task SendChat_WhenServerFails_ReturnsMessageOrThrowsAccordingToOptions()
        {
            using ChatCompletion client = new ChatCompletion(LoadSettings.ReadPropertyFromJson("ApiKey"));
            ReplaceHttpClient(client, new TestHttpMessageHandler(_ =>
                TestHttpMessageHandler.JsonResponse("{\"error\":\"bad request\"}", System.Net.HttpStatusCode.BadRequest)));

            string message = await client.SendChat("question");
            Assert.Contains("400", message);

            await Assert.ThrowsAsync<HttpRequestException>(() => client.SendChat("question", throwExceptionWhenErrorOccours: true));
        }

        private static void AddConversationPair(ChatCompletion client, string question, string answer)
        {
            List<PreviousQuestionAnswerPair> pairs = GetField<List<PreviousQuestionAnswerPair>>(client, "_previousQuestionsAndAnswers");
            pairs.Add(new PreviousQuestionAnswerPair(question, answer));
        }

        private static string ChatResponseJson(string content, int promptTokens, int completionTokens, int totalTokens)
        {
            return $"{{\"id\":\"test-id\",\"object\":\"chat.completion\",\"created\":1,\"model\":\"test-model\",\"choices\":[{{\"index\":0,\"message\":{{\"role\":\"assistant\",\"content\":\"{content}\"}},\"finish_reason\":\"stop\"}}],\"usage\":{{\"prompt_tokens\":{promptTokens},\"completion_tokens\":{completionTokens},\"total_tokens\":{totalTokens}}}}}";
        }

        private static void ReplaceHttpClient(ChatCompletion client, HttpMessageHandler handler)
        {
            FieldInfo field = typeof(AurelsOpenAIClient.OpenAiCommonBase).GetField("_httpClient", BindingFlags.Instance | BindingFlags.NonPublic);
            field.SetValue(client, new HttpClient(handler));
        }

        private static T GetField<T>(object instance, string fieldName)
        {
            FieldInfo field = FindField(instance.GetType(), fieldName);
            return (T)field.GetValue(instance);
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

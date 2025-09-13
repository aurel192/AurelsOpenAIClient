using AurelsOpenAIClient.Chat;

namespace AurelsOpenAIClient.Tests
{
    public class ChatCompletionEndpointTests
    {
        [Fact]
        public async Task SendChat_ValidRequest_ReturnsResponseContainingHello()
        {
            string apiKey = LoadSettings.LoadApiKeyFromJson("ApiKey");
            ChatCompletion chatClient = new ChatCompletion(apiKey);
            chatClient.SetModel("gpt-4.1-nano");

            string response = await chatClient.SendChat("Just Say 'Hello!'");

            // Basic assertions to check if the response is not null or empty and contains "hello"
            Assert.False(string.IsNullOrWhiteSpace(response));
            Assert.Contains("hello", response, StringComparison.OrdinalIgnoreCase);
        }

        // In this test we set a system role to reverse the characters in the response, it may fail if the model does not follow the system role instruction
        [Fact]
        public async Task SendChat_ReversedRequest_ReturnsResponseContainingReversedString()
        {
            string apiKey = LoadSettings.LoadApiKeyFromJson("ApiKey");
            ChatCompletion chatClient = new ChatCompletion(apiKey);
            chatClient.SetModel("gpt-4.1-nano");
            chatClient.SetSystemRole("Your response is characters backwards.");

            string response = await chatClient.SendChat("Just Say 'hello'!"); // ",But your job is to spell the letters backwards!" To make it easier for the model to understand

            // Basic assertions to check if the response is not null or empty and contains "olleh"
            Assert.False(string.IsNullOrWhiteSpace(response));
            Assert.Contains("olleh", response, StringComparison.OrdinalIgnoreCase);
        }
    }
}

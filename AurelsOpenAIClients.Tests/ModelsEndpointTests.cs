using System.Text.Json;
using AurelsOpenAIClient.ModelList;

namespace AurelsOpenAIClient.Tests
{
    public class ModelsEndpointTests
    {
        [Fact]
        public async Task GetModels_ValidApiKey_ReturnsExpectedJsonStructure()
        {
            // Arrange
            string apiKey = LoadSettings.ReadPropertyFromJson("ApiKey");
            Models models = new Models(apiKey);

            // Act
            string availableModels = await models.GetModels();

            // Assert basic string validity
            Assert.False(string.IsNullOrWhiteSpace(availableModels));

            // Parse JSON and validate structure matches the sample:
            // {
            //   "object": "list",
            //   "data": [ { "id": "...", "object": "model", "created": 123, "owned_by": "..." }, ... ]
            // }

            // NOTE: Schema may evolve, and properties may be added or renamed!
            using JsonDocument doc = JsonDocument.Parse(availableModels);
            JsonElement root = doc.RootElement;

            Assert.Equal(JsonValueKind.Object, root.ValueKind);

            // "object" property exists and equals "list" (tolerant check)
            Assert.True(root.TryGetProperty("object", out var objectProp) && objectProp.ValueKind == JsonValueKind.String);
            Assert.Equal("list", objectProp.GetString());

            // "data" property exists and is an array with at least one element
            Assert.True(root.TryGetProperty("data", out var dataProp), "'data' property missing");
            Assert.Equal(JsonValueKind.Array, dataProp.ValueKind);
            Assert.True(dataProp.GetArrayLength() > 0, "'data' array is empty");

            // Inspect first item for required properties
            JsonElement first = dataProp[0];
            Assert.Equal(JsonValueKind.Object, first.ValueKind);

            Assert.True(first.TryGetProperty("id", out var idProp) && idProp.ValueKind == JsonValueKind.String);
            Assert.False(string.IsNullOrWhiteSpace(idProp.GetString()));

            Assert.True(first.TryGetProperty("object", out var modelObjectProp) && modelObjectProp.ValueKind == JsonValueKind.String);
            Assert.False(string.IsNullOrWhiteSpace(modelObjectProp.GetString()));

            Assert.True(first.TryGetProperty("created", out var createdProp) && createdProp.ValueKind == JsonValueKind.Number);

            Assert.True(first.TryGetProperty("owned_by", out var ownedByProp) && ownedByProp.ValueKind == JsonValueKind.String);
            Assert.False(string.IsNullOrWhiteSpace(ownedByProp.GetString()));
        }

        [Fact]
        public async Task GetModels_InvalidApiKey_ThrowsException()
        {
            Models models = new Models("IT_IS_NOT_A_VALID_API_KEY");

            await Assert.ThrowsAsync<ApplicationException>(() => models.GetModels());
        }
    }
}
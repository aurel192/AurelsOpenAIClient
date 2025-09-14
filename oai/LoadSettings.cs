using System.Text.Json;

namespace oai
{
    public static class LoadSettings
    {
        public static string? ReadPropertyFromJson(string propertyName, string configFileName = "settings.json")
        {
            string startDir = AppContext.BaseDirectory;
            string configPath = FindFileUp(startDir, configFileName);

            if (configPath == null)
            {
                return null;
            }

            using var doc = JsonDocument.Parse(File.ReadAllText(configPath));
            if (doc.RootElement.TryGetProperty("OpenAISettings", out var openAISettings) &&
                openAISettings.TryGetProperty(propertyName, out var propertyValue) &&
                propertyValue.ValueKind == JsonValueKind.String)
            {
                string? strValue = propertyValue.GetString();
                if (string.IsNullOrWhiteSpace(strValue))
                    return null;
                return strValue;
            }
            return null;
        }

        private static string FindFileUp(string startDir, string fileName)
        {
            var dir = new DirectoryInfo(startDir);
            while (dir != null)
            {
                var path = Path.Combine(dir.FullName, fileName);
                if (File.Exists(path))
                    return path;
                dir = dir.Parent;
            }
            return null;
        }
    }
}

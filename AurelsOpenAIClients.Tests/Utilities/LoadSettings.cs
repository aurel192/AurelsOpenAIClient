using System;
using System.IO;
using System.Text.Json;

namespace AurelsOpenAIClient.Tests
{
    public static class LoadSettings
    {
        public static string LoadApiKeyFromJson(string propertyName)
        {
            return ReadPropertyFromJson(propertyName);
        }

        public static string LoadSampleAudioFromJson(string propertyName)
        {
            return ReadPropertyFromJson(propertyName);
        }

        private static string ReadPropertyFromJson(string propertyName, string configFileName = "testsettings.json")
        {
            string startDir = AppContext.BaseDirectory;
            string configPath = FindFileUp(startDir, configFileName);

            if (configPath == null)
            {
                throw new FileNotFoundException(
                    $"Configuration file '{configFileName}' not found. Place it in the test project folder or a parent folder!");
            }

            using var doc = JsonDocument.Parse(File.ReadAllText(configPath));
            if (doc.RootElement.TryGetProperty("OpenAI", out var openAiSection) &&
                openAiSection.TryGetProperty(propertyName, out var propertyValue) &&
                propertyValue.ValueKind == JsonValueKind.String)
            {
                string? strValue = propertyValue.GetString();
                if (string.IsNullOrWhiteSpace(strValue))
                    throw new InvalidOperationException($"Configuration property '{propertyName}' in '{configFileName}' is empty.");
                return strValue;
            }

            throw new InvalidOperationException($"Property '{propertyName}' not found in configuration file. Expected structure: {{ \"OpenAI\": {{ \"{propertyName}\": \"...\" }} }}");
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
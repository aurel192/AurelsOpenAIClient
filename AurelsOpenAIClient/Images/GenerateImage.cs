using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using AurelsOpenAIClient.Images.Request;
using AurelsOpenAIClient.Images.Response;

namespace AurelsOpenAIClient.Images
{
    /// <summary>
    /// Image generation via OpenAI Images API
    /// Endpoint: https://api.openai.com/v1/images/generations
    /// Docs: https://platform.openai.com/docs/api-reference/images/create
    /// </summary>
    public class GenerateImage : OpenAiCommonBase
    {
        public GenerateImage(string apiKey, string organization = null, string project = null) : base(apiKey, organization, project)
        {
            _endpoint = "https://api.openai.com/v1/images/generations";
            _model = "dall-e-3"; // default model
        }

        /// <summary>
        /// Generate one or more images from a prompt.
        /// Returns the filenames of saved images.
        /// If responseFormat == "b64_json" images are decoded from base64 and saved.
        /// If responseFormat == "url" images are downloaded from returned URLs.
        /// </summary>
        /// <param name="prompt">Prompt text</param>
        /// <param name="n">Number of images to generate (1..10 typically)</param>
        /// <param name="size">Size like "1024x1024", "512x512", "256x256"</param>
        /// <param name="outputFileName">Base filename (if n>1, suffixes _1,_2 will be added)</param>
        /// <returns>array of saved file names</returns>
        public async Task<string[]> Generate(string prompt, string outputFileName = "GeneratedImage.png", string size = "1024x1024", int n = 1)
        {
            CheckParameters(prompt, n, size, outputFileName);

            try
            {
                // Create request object
                CreateImageRequest request = new CreateImageRequest(_model, prompt, size, n, output_format: "png", quality: "auto", output_compression: 100);

                // Serialize request to JSON
                string jsonRequest = JsonSerializer.Serialize(request);
                StringContent content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

                // Send POST request
                HttpResponseMessage response = await _httpClient.PostAsync(_endpoint, content);
                response.EnsureSuccessStatusCode();

                // Parse response
                string jsonResponse = await response.Content.ReadAsStringAsync();

                JsonSerializerOptions options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                CreateImageResponse imageResponse = JsonSerializer.Deserialize<CreateImageResponse>(jsonResponse, options);

                // Check if deserialization was successful
                if (imageResponse == null || imageResponse.data == null || imageResponse.data.Length == 0)
                {
                    throw new ApplicationException("Failed to deserialize image response or no images returned.");
                }

                // Save images and collect filenames
                List<string> savedFiles = new List<string>();

                for (int i = 0; i < imageResponse.data.Length; i++)
                {
                    string fileName = imageResponse.data.Length == 1 ? outputFileName :
                        Path.GetFileNameWithoutExtension(outputFileName) + $"_{i + 1}" + Path.GetExtension(outputFileName);

                    string fullPath = Path.GetFullPath(fileName);

                    if (File.Exists(fullPath))
                    {
                        throw new ApplicationException($"File already exists: {fullPath}. Use a different filename!!");
                    }

                    // Check if base64 data is available
                    if (!string.IsNullOrEmpty(imageResponse.data[i].b64_json))
                    {
                        // Save image from base64 data
                        SaveBase64Image(imageResponse.data[i].b64_json, fullPath);
                    }
                    // Check if URL is available
                    else if (!string.IsNullOrEmpty(imageResponse.data[i].url))
                    {
                        // Download and save image from URL
                        await DownloadAndSave(imageResponse.data[i].url, fullPath);
                    }
                    else
                    {
                        throw new ApplicationException($"No URL or base64 data found for image {i + 1}");
                    }

                    savedFiles.Add(fileName);
                }

                return savedFiles.ToArray();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void CheckParameters(string prompt, int n, string size, string outputFileName)
        {
            if (string.IsNullOrEmpty(prompt))
                throw new ApplicationException("OpenAI: prompt is not set.");

            if (n <= 0)
                throw new ApplicationException("n must be at least 1.");

            if (string.IsNullOrEmpty(size))
                throw new ApplicationException("size is not set.");

            if (string.IsNullOrEmpty(outputFileName))
                throw new ApplicationException("outputFileName is not set.");

            if (string.IsNullOrEmpty(_model))
                throw new ApplicationException("OpenAI Model is not set.");
        }

        private async Task DownloadAndSave(string url, string pathToSave)
        {
            HttpClient client = new HttpClient();
            HttpResponseMessage resp = await client.GetAsync(url);
            resp.EnsureSuccessStatusCode();
            byte[] data = await resp.Content.ReadAsByteArrayAsync();
            File.WriteAllBytes(pathToSave, data);
            client.Dispose();
        }

        private void SaveBase64Image(string base64Data, string pathToSave)
        {
            try
            {
                byte[] imageBytes = Convert.FromBase64String(base64Data);
                File.WriteAllBytes(pathToSave, imageBytes);
            }
            catch (Exception ex)
            {
                throw new ApplicationException($"Failed to save base64 image: {ex.Message}", ex);
            }
        }
    }
}

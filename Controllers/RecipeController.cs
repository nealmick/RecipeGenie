using Microsoft.AspNetCore.Mvc;
using RecipeGenerator.Data;
using RecipeGenerator.Models;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using System.Text.Json.Serialization;

using Amazon.S3;
using Amazon.S3.Model;
using System.IO;


namespace RecipeGenerator.Controllers
{
    public class RecipeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly HttpClient _httpClient;
        private readonly ILogger<RecipeController> _logger;
        private readonly string _openAiApiKey;

        public RecipeController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, ILogger<RecipeController> logger)
        {
            _context = context;
            _userManager = userManager;
            _httpClient = new HttpClient();
            _logger = logger;
            _openAiApiKey = "redacted";

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string prompt)
        {
            if (!ModelState.IsValid)
            {
                return RedirectToAction("Index", "Home", new { error = "Model state is invalid" });
            }

            var recipe = new Recipe
            {
                Prompt = prompt,
                Timestamp = DateTime.UtcNow,
                UserId = User.Identity.IsAuthenticated ? _userManager.GetUserId(User) : null
            };

            try
            {
                var recipeResponse = await GenerateRecipeAsync(prompt);
                if (recipeResponse == null)
                {
                    _logger.LogError("Recipe generation failed, no response was returned.");
                    return RedirectToAction("Index", "Home", new { error = "Failed to generate recipe" });
                }

                recipe.Name = recipeResponse.Name;
                recipe.Content = recipeResponse.Instructions;

                _logger.LogInformation("Generated image prompt: {ImagePrompt}", recipeResponse.ImagePrompt);
                var imageUrl = await GenerateImageAsync(recipeResponse.ImagePrompt);
                if (!string.IsNullOrEmpty(imageUrl))
                {
                    _logger.LogInformation("Generated image URL: {ImageUrl}", imageUrl);
                    var permanentImageUrl = await UploadImageToR2AndGetPublicUrl(imageUrl);
                    if (!string.IsNullOrEmpty(permanentImageUrl))
                    {
                        recipe.ImageUrl = permanentImageUrl;
                        _logger.LogInformation("Uploaded image to R2, permanent URL: {PermanentImageUrl}", permanentImageUrl);
                    }
                    else
                    {
                        _logger.LogWarning("Failed to upload image to R2. Proceeding without an image.");
                    }
                }
                else
                {
                    _logger.LogWarning("Image URL is empty after generation. Proceeding without an image.");
                }

                _context.Recipes.Add(recipe);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index", "Detail", new { id = recipe.Id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while generating the recipe");
                return RedirectToAction("Index", "Home", new { error = "An error occurred while generating the recipe" });
            }
        }

        private async Task<string> UploadImageToR2AndGetPublicUrl(string imageUrl)
        {
            // Directly embedded credentials for demonstration purposes only
            var accessKey = "redacted";
            var secretKey = "redacted";
            var bucketName = "redacted";
            var accountId = "redacted";  // This needs to be your Cloudflare R2 Account ID if different
            var serviceURL = $"https://{accountId}.r2.cloudflarestorage.com";
            var publicURL = "redacted";

            var client = new AmazonS3Client(accessKey, secretKey, new AmazonS3Config
            {
                ServiceURL = serviceURL,
                ForcePathStyle = true,  // Ensure to use path-style access
                UseHttp = true  // Optional: Use HTTP instead of HTTPS if required by your R2 setup
            });

            try
            {
                var httpClient = new HttpClient();  // Ideally, HttpClient should be injected
                var imageData = await httpClient.GetByteArrayAsync(imageUrl);
                if (imageData == null || imageData.Length == 0)
                {
                    Console.WriteLine("No image data could be downloaded from the provided URL.");
                    return null;
                }

                var uniqueFileName = $"{Guid.NewGuid()}.jpg";
                Console.WriteLine($"Image downloaded successfully. File size: {imageData.Length} bytes");

                var putRequest = new Amazon.S3.Model.PutObjectRequest
                {
                    BucketName = bucketName,
                    Key = uniqueFileName,
                    InputStream = new MemoryStream(imageData),
                    ContentType = "image/jpeg",
                    DisablePayloadSigning = true
                };

                Console.WriteLine("Attempting to upload the image to R2 bucket.");
                var response = await client.PutObjectAsync(putRequest);
                if (response.HttpStatusCode == System.Net.HttpStatusCode.OK)
                {
                    var uploadedUrl = $"{publicURL}/{uniqueFileName}";

                    Console.WriteLine($"Image uploaded successfully to {uploadedUrl}");
                    return uploadedUrl;
                }
                else
                {
                    Console.WriteLine($"Failed to upload image to Cloudflare R2: {response.HttpStatusCode}");
                    return null;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while uploading the image to R2: {ex.Message}");
                return null;
            }
        }




        private async Task<RecipeResponse> GenerateRecipeAsync(string prompt)
        {
            var requestBody = new
            {
                model = "gpt-4o",
                messages = new[]
                {
                    new { role = "system", content = "You are a recipe and delicious food generator. You will be given a message from the user describing a request or some ingredients. Your job is to reply in JSON format only. The JSON object should be formatted with 3 keys: recipe_name, recipe_content, and recipe_image. You should fill in each JSON key with content based on the user's request. The image should be a text description of what the final recipe should look like, the content should be the instructions, and the name is up to you to decide based on the user's prompt. Remember to format your response as described." },
                    new { role = "user", content = prompt }
                }
            };

            var responseJson = await SendHttpRequestAsync("https://api.openai.com/v1/chat/completions", requestBody);
            if (string.IsNullOrEmpty(responseJson))
            {
                _logger.LogError("Empty response received from the recipe generation API");
                return null;
            }

            var completionResponse = JsonSerializer.Deserialize<CompletionResponse>(responseJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            if (completionResponse?.Choices == null || completionResponse.Choices.Count == 0)
            {
                _logger.LogError("Failed to extract recipe content. The response might be missing expected data.");
                return null;
            }

            var messageContent = completionResponse.Choices[0].Message?.Content;
            if (string.IsNullOrEmpty(messageContent))
            {
                _logger.LogError("Recipe content is empty");
                return null;
            }

            var jsonStartIndex = messageContent.IndexOf("{");
            var jsonEndIndex = messageContent.LastIndexOf("}") + 1;
            var jsonString = messageContent.Substring(jsonStartIndex, jsonEndIndex - jsonStartIndex).Trim();

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            var recipeResponse = JsonSerializer.Deserialize<RecipeResponse>(jsonString, options);

            _logger.LogInformation("Recipe Name: {Name}", recipeResponse?.Name);
            _logger.LogInformation("Recipe Instructions: {Instructions}", recipeResponse?.Instructions);
            _logger.LogInformation("DALL-E Image Prompt: {ImagePrompt}", recipeResponse?.ImagePrompt);

            return recipeResponse;
        }
        private async Task<string> GenerateImageAsync(string imagePrompt)
        {
            if (string.IsNullOrEmpty(imagePrompt))
            {
                _logger.LogWarning("Image prompt is empty. Skipping image generation.");
                return null;
            }

            var imageRequestBody = new
            {
                model = "dall-e-3",
                prompt = imagePrompt,
                n = 1,
                size = "1024x1024"
            };

            var imageResponseJson = await SendHttpRequestAsync("https://api.openai.com/v1/images/generations", imageRequestBody);
            _logger.LogInformation("Image API Response: {ImageResponseJson}", imageResponseJson);

            if (string.IsNullOrEmpty(imageResponseJson))
            {
                _logger.LogError("Empty response received from the image generation API");
                return null;
            }

            try
            {
                var imageResponse = JsonSerializer.Deserialize<ImageGenerationResponse>(imageResponseJson);
                if (imageResponse?.Data == null || imageResponse.Data.Count == 0)
                {
                    _logger.LogError("Failed to extract image data. The response might be missing expected data.");
                    return null;
                }

                var imageUrl = imageResponse.Data[0]?.Url;
                if (string.IsNullOrEmpty(imageUrl))
                {
                    _logger.LogError("Image URL is empty");
                    return null;
                }

                _logger.LogInformation("Image URL: {ImageUrl}", imageUrl);
                return imageUrl;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while deserializing the image response");
                return null;
            }
        }
        private async Task<string> SendHttpRequestAsync(string uri, object requestBody)
        {
            var requestJson = JsonSerializer.Serialize(requestBody);
            var requestContent = new StringContent(requestJson, Encoding.UTF8, "application/json");

            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, uri)
            {
                Headers = { { "Authorization", $"Bearer {_openAiApiKey}" } },
                Content = requestContent
            };

            try
            {
                var httpResponseMessage = await _httpClient.SendAsync(httpRequestMessage);
                if (httpResponseMessage.IsSuccessStatusCode)
                {
                    return await httpResponseMessage.Content.ReadAsStringAsync();
                }
                else
                {
                    _logger.LogError("HTTP request failed with status code: {StatusCode}", httpResponseMessage.StatusCode);
                    return null;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while sending the HTTP request");
                return null;
            }
        }



        private class CompletionResponse
        {
            public List<Choice> Choices { get; set; }
        }

        private class Choice
        {
            public Message Message { get; set; }
        }

        public class Message
        {
            public string Role { get; set; }
            public string Content { get; set; }
        }

        private class RecipeResponse
        {
            [JsonPropertyName("recipe_name")]
            public string Name { get; set; }

            [JsonPropertyName("recipe_content")]
            public string Instructions { get; set; }

            [JsonPropertyName("recipe_image")]
            public string ImagePrompt { get; set; }
        }
        private class ImageGenerationResponse
        {
            [JsonPropertyName("data")]
            public List<ImageData> Data { get; set; }
        }

        private class ImageData
        {
            [JsonPropertyName("url")]
            public string Url { get; set; }
        }
    }
}

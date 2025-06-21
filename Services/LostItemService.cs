using System.Text.Json;
using System.Text;
using WaslAlkhair.Api.Repositories.Interfaces;
using WaslAlkhair.Api.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using WaslAlkhair.Api.DTOs.LostANDfound;

namespace WaslAlkhair.Api.Services
{
    public class LostItemService : ILostItemService
    {
        private readonly ILogger<LostItemService> _logger;
        private readonly string _flaskApiUrl;
        private readonly ILostItemRepository _itemRepository;
        private readonly IFileService _cloudinaryService;
        private readonly HttpClient _httpClient;
        private readonly int _timeoutSeconds;

        public LostItemService(
            ILogger<LostItemService> logger,
            IConfiguration configuration,
            ILostItemRepository itemRepository,
            IFileService cloudinaryService,
            HttpClient httpClient)
        {
            _logger = logger;
            _flaskApiUrl = configuration["FlaskApi:BaseUrl"] ?? "https://norhannnabil-clip-image-search.hf.space";
            _timeoutSeconds = configuration.GetValue<int>("FlaskApi:TimeoutSeconds", 120);
            _itemRepository = itemRepository;
            _cloudinaryService = cloudinaryService;
            _httpClient = httpClient;

            _httpClient.Timeout = TimeSpan.FromSeconds(_timeoutSeconds);
            _logger.LogInformation($"LostItemService initialized with Flask API URL: {_flaskApiUrl}");
        }

        public async Task<LostItem> CreateItemWithImageAsync(IFormFile image, string metadata, string location, DateTime date, ItemType type, string contactInfo, string userId)
        {
            try
            {
                // Upload image to Cloudinary
                var cloudinaryUrl = await _cloudinaryService.UploadFileAsync(image);
                if (string.IsNullOrEmpty(cloudinaryUrl))
                {
                    _logger.LogError("Failed to upload image to Cloudinary");
                    return null;
                }

                // Read image data for embedding generation
                using var stream = new MemoryStream();
                await image.CopyToAsync(stream);
                var imageData = stream.ToArray();

                // Generate embedding
                var embedding = await GenerateImageEmbeddingAsync(imageData);
                if (embedding == null)
                {
                    // If embedding generation fails, delete the uploaded image
                    await _cloudinaryService.DeleteFileAsync(ExtractFileNameFromUrl(cloudinaryUrl));
                    _logger.LogError("Failed to generate embedding for uploaded image");
                    return null;
                }

                // Create lost item object
                var lostItem = new LostItem
                {
                    Id = Guid.NewGuid(),
                    ImagePath = cloudinaryUrl,
                    Embedding = JsonSerializer.Serialize(embedding),
                    Metadata = metadata,
                    Location = location,
                    Date = date,
                    Type = type,
                    ContactInfo = contactInfo,
                    UserId = userId,
                    CreatedAt = DateTime.UtcNow,
                    IsResolved = false
                };

                // Save to database
                await _itemRepository.SaveItemWithEmbeddingAsync(lostItem);

                _logger.LogInformation($"Lost item created successfully: {lostItem.Id}");
                return lostItem;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating lost item with image");
                return null;
            }
        }

        public async Task<List<float>> GenerateImageEmbeddingAsync(byte[] imageData)
        {
            try
            {
                var isReady = await WaitForModelReadyAsync();
                if (!isReady)
                {
                    _logger.LogError("Flask API model is not ready");
                    return null;
                }

                var base64Image = Convert.ToBase64String(imageData);
                var requestData = new { image = base64Image };
                var jsonContent = JsonSerializer.Serialize(requestData);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync($"{_flaskApiUrl}/extract", content);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError($"Flask API returned error status {response.StatusCode}: {errorContent}");
                    return null;
                }

                var responseContent = await response.Content.ReadAsStringAsync();
                var jsonDocument = JsonDocument.Parse(responseContent);
                var jsonElement = jsonDocument.RootElement;

                if (jsonElement.TryGetProperty("success", out var successElement) && successElement.GetBoolean())
                {
                    if (jsonElement.TryGetProperty("features", out var featuresElement))
                    {
                        var features = new List<float>();
                        if (featuresElement.ValueKind == JsonValueKind.Array)
                        {
                            foreach (var item in featuresElement.EnumerateArray())
                            {
                                if (item.ValueKind == JsonValueKind.Number)
                                {
                                    features.Add(item.GetSingle());
                                }
                            }
                        }

                        _logger.LogInformation($"Successfully extracted {features.Count} features from image");
                        return features;
                    }
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating image embedding");
                return null;
            }
        }

        public async Task<List<LostItemSearchResult>> SearchSimilarItemsAsync(List<float> queryEmbedding)
        {
            try
            {
                var results = new List<LostItemSearchResult>();
                var storedItems = await _itemRepository.GetAllItemsWithEmbeddingsAsync();

                foreach (var item in storedItems)
                {
                    if (!string.IsNullOrEmpty(item.Embedding))
                    {
                        var embedding = JsonSerializer.Deserialize<List<float>>(item.Embedding);
                        if (embedding != null && embedding.Any())
                        {
                            var similarity = await CalculateSimilarityAsync(queryEmbedding, embedding);

                            if (similarity > 0.7)
                            {
                                results.Add(new LostItemSearchResult
                                {
                                    ItemId = item.Id,
                                    ImagePath = item.ImagePath,
                                    Similarity = similarity,
                                    Metadata = item.Metadata,
                                    Location = item.Location,
                                    Date = item.Date,
                                    Type = item.Type,
                                    ContactInfo = item.ContactInfo,
                                    IsResolved = item.IsResolved
                                });
                            }
                        }
                    }
                }

                return results.OrderByDescending(x => x.Similarity).Take(20).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching similar items");
                return new List<LostItemSearchResult>();
            }
        }

        public async Task<double> CalculateSimilarityAsync(List<float> features1, List<float> features2)
        {
            try
            {
                var requestData = new
                {
                    features1 = features1,
                    features2 = features2
                };

                var jsonContent = JsonSerializer.Serialize(requestData);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync($"{_flaskApiUrl}/similarity", content);

                if (!response.IsSuccessStatusCode)
                {
                    return 0.0;
                }

                var responseContent = await response.Content.ReadAsStringAsync();
                var jsonDocument = JsonDocument.Parse(responseContent);
                var jsonElement = jsonDocument.RootElement;

                if (jsonElement.TryGetProperty("success", out var successElement) && successElement.GetBoolean())
                {
                    if (jsonElement.TryGetProperty("similarity", out var similarityElement))
                    {
                        return similarityElement.GetDouble();
                    }
                }

                return 0.0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating similarity");
                return 0.0;
            }
        }

        private async Task<bool> WaitForModelReadyAsync(int maxRetries = 3)
        {
            for (int attempt = 1; attempt <= maxRetries; attempt++)
            {
                try
                {
                    _logger.LogInformation($"Checking if Flask API model is ready (attempt {attempt}/{maxRetries})");

                    var response = await _httpClient.GetAsync($"{_flaskApiUrl}/wait-ready?timeout=60");

                    if (response.IsSuccessStatusCode)
                    {
                        var responseContent = await response.Content.ReadAsStringAsync();
                        var jsonDocument = JsonDocument.Parse(responseContent);
                        var jsonElement = jsonDocument.RootElement;

                        if (jsonElement.TryGetProperty("ready", out var readyElement) && readyElement.GetBoolean())
                        {
                            _logger.LogInformation("Flask API model is ready");
                            return true;
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error checking Flask API readiness (attempt {attempt}/{maxRetries})");
                }

                if (attempt < maxRetries)
                {
                    await Task.Delay(TimeSpan.FromSeconds(5));
                }
            }

            return false;
        }

        private string ExtractFileNameFromUrl(string cloudinaryUrl)
        {
            try
            {
                var uri = new Uri(cloudinaryUrl);
                return Path.GetFileNameWithoutExtension(uri.LocalPath);
            }
            catch
            {
                return string.Empty;
            }
        }
    }
}
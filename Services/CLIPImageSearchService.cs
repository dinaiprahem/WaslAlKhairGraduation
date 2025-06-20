using System.Text.Json;
using System.Text;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using WaslAlkhair.Api.Data;
using System.Net.Http;
using System.Text;
using WaslAlkhair.Api.Repositories.Interfaces;
using WaslAlkhair.Api.DTOs.LostANDfound;

namespace WaslAlkhair.Api.Services
{
    public class CLIPImageSearchService : IImageSearchService
    {
        private readonly ILogger<CLIPImageSearchService> _logger;
        private readonly string _flaskApiUrl;
        private readonly IImageRepository _imageRepository;
        private readonly IFileService _cloudinaryService;
        private readonly HttpClient _httpClient;
        private readonly int _timeoutSeconds;

        public CLIPImageSearchService(
            ILogger<CLIPImageSearchService> logger,
            IConfiguration configuration,
            IImageRepository imageRepository,
            IFileService cloudinaryService,
            HttpClient httpClient)
        {
            _logger = logger;
            _flaskApiUrl = configuration["FlaskApi:BaseUrl"] ?? "https://norhannnabil-clip-image-search.hf.space";
            _timeoutSeconds = configuration.GetValue<int>("FlaskApi:TimeoutSeconds", 120);
            _imageRepository = imageRepository;
            _cloudinaryService = cloudinaryService;
            _httpClient = httpClient;

            // Configure HttpClient timeout
            _httpClient.Timeout = TimeSpan.FromSeconds(_timeoutSeconds);

            _logger.LogInformation($"CLIPImageSearchService initialized with Flask API URL: {_flaskApiUrl}");
        }

        public async Task<StoredImage> UploadAndProcessImageAsync(IFormFile image, string metadata = null)
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

                // Create stored image object
                var storedImage = new StoredImage
                {
                    Id = Guid.NewGuid(),
                    ImagePath = cloudinaryUrl,
                    Embedding = embedding,
                    Metadata = metadata,
                    CreatedAt = DateTime.UtcNow
                };

                // Save to database
                await _imageRepository.SaveImageWithEmbeddingAsync(storedImage);

                _logger.LogInformation($"Image uploaded and processed successfully: {storedImage.Id}");
                return storedImage;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading and processing image");
                return null;
            }
        }

        public async Task<List<float>> GenerateImageEmbeddingAsync(byte[] imageData)
        {
            try
            {
                // Wait for the model to be ready
                var isReady = await WaitForModelReadyAsync();
                if (!isReady)
                {
                    _logger.LogError("Flask API model is not ready");
                    return null;
                }

                var base64Image = Convert.ToBase64String(imageData);

                var requestData = new
                {
                    image = base64Image
                };

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

                if (jsonElement.TryGetProperty("success", out var successElement) &&
                    successElement.GetBoolean())
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
                    else
                    {
                        _logger.LogError("Features property not found in response");
                        return null;
                    }
                }
                else
                {
                    var error = jsonElement.TryGetProperty("error", out var errorElement)
                        ? errorElement.GetString()
                        : "Unknown error";
                    _logger.LogError($"Failed to extract features: {error}");
                    return null;
                }
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP error while calling Flask API for image embedding");
                return null;
            }
            catch (TaskCanceledException ex)
            {
                _logger.LogError(ex, "Timeout while calling Flask API for image embedding");
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating image embedding");
                return null;
            }
        }

        public async Task<List<SearchResult>> SearchSimilarImagesAsync(List<float> queryEmbedding)
        {
            try
            {
                var results = new List<SearchResult>();
                var storedImages = await _imageRepository.GetAllImagesWithEmbeddingsAsync();

                foreach (var storedImage in storedImages)
                {
                    if (storedImage.Embedding != null && storedImage.Embedding.Any())
                    {
                        var similarity = await CalculateSimilarityAsync(queryEmbedding, storedImage.Embedding);

                        if (similarity > 0.7)
                        {
                            results.Add(new SearchResult
                            {
                                ImageId = storedImage.Id,
                                ImagePath = storedImage.ImagePath,
                                Similarity = similarity,
                                Metadata = storedImage.Metadata
                            });
                        }
                    }
                }

                return results.OrderByDescending(x => x.Similarity).Take(20).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching similar images");
                return new List<SearchResult>();
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
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError($"Flask API returned error status {response.StatusCode}: {errorContent}");
                    return 0.0;
                }

                var responseContent = await response.Content.ReadAsStringAsync();
                var jsonDocument = JsonDocument.Parse(responseContent);
                var jsonElement = jsonDocument.RootElement;

                if (jsonElement.TryGetProperty("success", out var successElement) &&
                    successElement.GetBoolean())
                {
                    if (jsonElement.TryGetProperty("similarity", out var similarityElement))
                    {
                        return similarityElement.GetDouble();
                    }
                }

                return 0.0;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP error while calling Flask API for similarity calculation");
                return 0.0;
            }
            catch (TaskCanceledException ex)
            {
                _logger.LogError(ex, "Timeout while calling Flask API for similarity calculation");
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

                        if (jsonElement.TryGetProperty("ready", out var readyElement) &&
                            readyElement.GetBoolean())
                        {
                            _logger.LogInformation("Flask API model is ready");
                            return true;
                        }
                        else
                        {
                            var error = jsonElement.TryGetProperty("error", out var errorElement)
                                ? errorElement.GetString()
                                : "Model not ready";
                            _logger.LogWarning($"Flask API model not ready: {error}");
                        }
                    }
                    else
                    {
                        _logger.LogWarning($"Flask API health check failed with status {response.StatusCode}");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error checking Flask API readiness (attempt {attempt}/{maxRetries})");
                }

                // Wait before retry (except on last attempt)
                if (attempt < maxRetries)
                {
                    await Task.Delay(TimeSpan.FromSeconds(5));
                }
            }

            _logger.LogError("Flask API model is not ready after all retry attempts");
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

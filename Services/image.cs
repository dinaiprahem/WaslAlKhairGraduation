namespace WaslAlkhair.Api.Services
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Text.Json;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Microsoft.EntityFrameworkCore;
    using WaslAlkhair.Api.Data;

    public interface IImageSearchService
    {
        Task<List<float>> GenerateImageEmbeddingAsync(byte[] imageData);
        Task<List<SearchResult>> SearchSimilarImagesAsync(List<float> queryEmbedding);
        Task<double> CalculateSimilarityAsync(List<float> features1, List<float> features2);
        Task<StoredImage> UploadAndProcessImageAsync(IFormFile image, string metadata = null);
    }

    public class CLIPImageSearchService : IImageSearchService
    {
        private readonly ILogger<CLIPImageSearchService> _logger;
        private readonly string _pythonPath;
        private readonly string _scriptPath;
        private readonly IImageRepository _imageRepository;
        private readonly IFileService _cloudinaryService;

        public CLIPImageSearchService(
            ILogger<CLIPImageSearchService> logger,
            IConfiguration configuration,
            IImageRepository imageRepository,
            IFileService cloudinaryService)
        {
            _logger = logger;
            _pythonPath = configuration["Python:ExecutablePath"] ?? "python";
            _scriptPath = configuration["Python:CLIPScriptPath"] ?? "ImageSearch.py";
            _imageRepository = imageRepository;
            _cloudinaryService = cloudinaryService;
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
                var base64Image = Convert.ToBase64String(imageData);
                var result = await RunPythonScriptAsync("extract", base64Image);

                if (result.HasValue)
                {
                    var jsonElement = result.Value;

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
                else
                {
                    _logger.LogError("No response received from Python script");
                    return null;
                }
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
                var features1Json = JsonSerializer.Serialize(features1);
                var features2Json = JsonSerializer.Serialize(features2);

                var result = await RunPythonScriptAsync("similarity", features1Json, features2Json);

                if (result.HasValue)
                {
                    var jsonElement = result.Value;

                    if (jsonElement.TryGetProperty("success", out var successElement) &&
                        successElement.GetBoolean())
                    {
                        if (jsonElement.TryGetProperty("similarity", out var similarityElement))
                        {
                            return similarityElement.GetDouble();
                        }
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

        private async Task<JsonElement?> RunPythonScriptAsync(params string[] arguments)
        {
            try
            {
                var argumentString = string.Join(" ", arguments.Select(arg => $"\"{arg}\""));

                var startInfo = new ProcessStartInfo
                {
                    FileName = _pythonPath,
                    Arguments = $"{_scriptPath} {argumentString}",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                };

                using var process = Process.Start(startInfo);

                var output = await process.StandardOutput.ReadToEndAsync();
                var error = await process.StandardError.ReadToEndAsync();

                await process.WaitForExitAsync();

                if (process.ExitCode != 0)
                {
                    _logger.LogError($"Python script failed with exit code {process.ExitCode}: {error}");
                    return null;
                }

                if (string.IsNullOrWhiteSpace(output))
                {
                    _logger.LogWarning("Python script returned empty output");
                    return null;
                }

                _logger.LogDebug($"Python script output: {output}");

                try
                {
                    var jsonDocument = JsonDocument.Parse(output);
                    return jsonDocument.RootElement;
                }
                catch (JsonException ex)
                {
                    _logger.LogError(ex, $"Failed to parse JSON output: {output}");
                    return null;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error running Python script");
                return null;
            }
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

    // Models
    public class SearchResult
    {
        public Guid ImageId { get; set; }
        public string ImagePath { get; set; }
        public double Similarity { get; set; }
        public string Metadata { get; set; }
    }

    public class StoredImage
    {
        public Guid Id { get; set; }
        public string ImagePath { get; set; } // Cloudinary URL
        public List<float> Embedding { get; set; }
        public string Metadata { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    // EF Core Entity
    public class ImageEntity
    {
        public Guid Id { get; set; }
        public string ImagePath { get; set; }
        public string Embedding { get; set; } // JSON string
        public string Metadata { get; set; }
        public DateTime CreatedAt { get; set; }
    }

   

    // Repository Interface
    public interface IImageRepository
    {
        Task<List<StoredImage>> GetAllImagesWithEmbeddingsAsync();
        Task SaveImageWithEmbeddingAsync(StoredImage image);
        Task<StoredImage> GetImageByIdAsync(Guid id);
        Task<bool> DeleteImageAsync(Guid id);
        Task<List<StoredImage>> GetImagesByMetadataAsync(string metadata);
        Task<int> GetTotalImagesCountAsync();
        Task<List<StoredImage>> GetRecentImagesAsync(int count = 10);
    }

    // EF Core Repository Implementation
    public class ImageRepository : IImageRepository
    {
        private readonly AppDbContext _context;
        private readonly ILogger<ImageRepository> _logger;

        public ImageRepository(AppDbContext context, ILogger<ImageRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<List<StoredImage>> GetAllImagesWithEmbeddingsAsync()
        {
            try
            {
                var entities = await _context.Images
                    .OrderByDescending(x => x.CreatedAt)
                    .ToListAsync();

                return entities.Select(MapToStoredImage).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving images with embeddings");
                return new List<StoredImage>();
            }
        }

        public async Task SaveImageWithEmbeddingAsync(StoredImage image)
        {
            try
            {
                var entity = new ImageEntity
                {
                    Id = image.Id,
                    ImagePath = image.ImagePath,
                    Embedding = JsonSerializer.Serialize(image.Embedding),
                    Metadata = image.Metadata,
                    CreatedAt = image.CreatedAt
                };

                _context.Images.Add(entity);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Successfully saved image: {image.Id}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error saving image: {image.Id}");
                throw;
            }
        }

        public async Task<StoredImage> GetImageByIdAsync(Guid id)
        {
            try
            {
                var entity = await _context.Images
                    .FirstOrDefaultAsync(x => x.Id == id);

                return entity != null ? MapToStoredImage(entity) : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving image by ID: {id}");
                return null;
            }
        }

        public async Task<bool> DeleteImageAsync(Guid id)
        {
            try
            {
                var entity = await _context.Images.FindAsync(id);
                if (entity == null)
                    return false;

                _context.Images.Remove(entity);
                var result = await _context.SaveChangesAsync();

                _logger.LogInformation($"Deleted image: {id}");
                return result > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting image: {id}");
                return false;
            }
        }

        public async Task<List<StoredImage>> GetImagesByMetadataAsync(string metadata)
        {
            try
            {
                var entities = await _context.Images
                    .Where(x => x.Metadata.Contains(metadata))
                    .OrderByDescending(x => x.CreatedAt)
                    .ToListAsync();

                return entities.Select(MapToStoredImage).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving images by metadata");
                return new List<StoredImage>();
            }
        }

        public async Task<int> GetTotalImagesCountAsync()
        {
            try
            {
                return await _context.Images.CountAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting total images count");
                return 0;
            }
        }

        public async Task<List<StoredImage>> GetRecentImagesAsync(int count = 10)
        {
            try
            {
                var entities = await _context.Images
                    .OrderByDescending(x => x.CreatedAt)
                    .Take(count)
                    .ToListAsync();

                return entities.Select(MapToStoredImage).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving recent images");
                return new List<StoredImage>();
            }
        }

        private StoredImage MapToStoredImage(ImageEntity entity)
        {
            return new StoredImage
            {
                Id = entity.Id,
                ImagePath = entity.ImagePath,
                Embedding = string.IsNullOrEmpty(entity.Embedding) ?
                    new List<float>() :
                    JsonSerializer.Deserialize<List<float>>(entity.Embedding),
                Metadata = entity.Metadata,
                CreatedAt = entity.CreatedAt
            };
        }
    }
}
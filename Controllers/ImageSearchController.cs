using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using WaslAlkhair.Api.Services;
using System.Linq;
using WaslAlkhair.Api.Repositories.Interfaces;

namespace WaslAlkhair.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ImageSearchController : ControllerBase
    {
        private readonly IImageSearchService _searchService;
        private readonly IImageRepository _imageRepository;
        private readonly IFileService _cloudinaryService;
        private readonly ILogger<ImageSearchController> _logger;

        public ImageSearchController(
            IImageSearchService searchService,
            IImageRepository imageRepository,
            IFileService cloudinaryService,
            ILogger<ImageSearchController> logger)
        {
            _searchService = searchService;
            _imageRepository = imageRepository;
            _cloudinaryService = cloudinaryService;
            _logger = logger;
        }

        [HttpPost("upload")]
        public async Task<IActionResult> UploadImage(IFormFile image, [FromForm] string metadata = null)
        {
            try
            {
                // Validate input
                var validationResult = ValidateImageFile(image);
                if (validationResult != null)
                    return validationResult;

                // Upload and process image
                var storedImage = await _searchService.UploadAndProcessImageAsync(image, metadata);

                if (storedImage == null)
                {
                    return StatusCode(500, new { error = "فشل في رفع ومعالجة الصورة" });
                }

                _logger.LogInformation($"Image uploaded successfully: {storedImage.Id}");

                return Ok(new
                {
                    imageId = storedImage.Id,
                    imagePath = storedImage.ImagePath,
                    embeddingSize = storedImage.Embedding?.Count ?? 0,
                    message = "تم رفع الصورة بنجاح"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading image");
                return StatusCode(500, new { error = "حدث خطأ أثناء رفع الصورة" });
            }
        }

        [HttpPost("search")]
        public async Task<IActionResult> SearchSimilarImages(IFormFile queryImage)
        {
            try
            {
                // Validate input
                var validationResult = ValidateImageFile(queryImage, "لم يتم رفع صورة للبحث");
                if (validationResult != null)
                    return validationResult;

                using var stream = new MemoryStream();
                await queryImage.CopyToAsync(stream);
                var imageData = stream.ToArray();

                // Generate embedding for query image
                var queryEmbedding = await _searchService.GenerateImageEmbeddingAsync(imageData);

                if (queryEmbedding == null)
                {
                    return StatusCode(500, new { error = "فشل في معالجة صورة البحث" });
                }

                // Search for similar images
                var similarImages = await _searchService.SearchSimilarImagesAsync(queryEmbedding);

                _logger.LogInformation($"Found {similarImages.Count} similar images");

                return Ok(new
                {
                    resultsCount = similarImages.Count,
                    results = similarImages.Select(r => new
                    {
                        imageId = r.ImageId,
                        imagePath = r.ImagePath,
                        similarity = Math.Round(r.Similarity, 4),
                        similarityPercentage = Math.Round(r.Similarity * 100, 2),
                        metadata = r.Metadata
                    }).ToList()
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching similar images");
                return StatusCode(500, new { error = "حدث خطأ أثناء البحث" });
            }
        }

        [HttpGet("image/{id}")]
        public async Task<IActionResult> GetImageById(Guid id)
        {
            try
            {
                var image = await _imageRepository.GetImageByIdAsync(id);

                if (image == null)
                {
                    return NotFound(new { error = "الصورة غير موجودة" });
                }

                return Ok(new
                {
                    imageId = image.Id,
                    imagePath = image.ImagePath,
                    metadata = image.Metadata,
                    createdAt = image.CreatedAt,
                    embeddingSize = image.Embedding?.Count ?? 0
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting image by ID: {id}");
                return StatusCode(500, new { error = "حدث خطأ أثناء جلب الصورة" });
            }
        }

        [HttpDelete("image/{id}")]
        public async Task<IActionResult> DeleteImage(Guid id)
        {
            try
            {
                // Get image details first
                var image = await _imageRepository.GetImageByIdAsync(id);
                if (image == null)
                {
                    return NotFound(new { error = "الصورة غير موجودة" });
                }

                // Extract filename from Cloudinary URL for deletion
                var fileName = ExtractFileNameFromUrl(image.ImagePath);

                // Delete from database
                var dbDeleted = await _imageRepository.DeleteImageAsync(id);

                // Delete from Cloudinary
                var cloudinaryDeleted = false;
                if (!string.IsNullOrEmpty(fileName))
                {
                    cloudinaryDeleted = await _cloudinaryService.DeleteFileAsync(fileName);
                }

                if (dbDeleted)
                {
                    _logger.LogInformation($"Image deleted: {id}, Cloudinary deleted: {cloudinaryDeleted}");
                    return Ok(new
                    {
                        message = "تم حذف الصورة بنجاح",
                        cloudinaryDeleted = cloudinaryDeleted
                    });
                }
                else
                {
                    return StatusCode(500, new { error = "فشل في حذف الصورة من قاعدة البيانات" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting image: {id}");
                return StatusCode(500, new { error = "حدث خطأ أثناء حذف الصورة" });
            }
        }

        [HttpPost("compare")]
        public async Task<IActionResult> CompareImages(IFormFile image1, IFormFile image2)
        {
            try
            {
                if (image1 == null || image2 == null)
                {
                    return BadRequest(new { error = "يجب رفع صورتين للمقارنة" });
                }

                // Process first image
                using var stream1 = new MemoryStream();
                await image1.CopyToAsync(stream1);
                var embedding1 = await _searchService.GenerateImageEmbeddingAsync(stream1.ToArray());

                // Process second image
                using var stream2 = new MemoryStream();
                await image2.CopyToAsync(stream2);
                var embedding2 = await _searchService.GenerateImageEmbeddingAsync(stream2.ToArray());

                if (embedding1 == null || embedding2 == null)
                {
                    return StatusCode(500, new { error = "فشل في معالجة إحدى الصور" });
                }

                // Calculate similarity
                var similarity = await _searchService.CalculateSimilarityAsync(embedding1, embedding2);

                return Ok(new
                {
                    similarity = Math.Round(similarity, 4),
                    similarityPercentage = Math.Round(similarity * 100, 2),
                    interpretation = GetSimilarityInterpretation(similarity)
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error comparing images");
                return StatusCode(500, new { error = "حدث خطأ أثناء مقارنة الصور" });
            }
        }

        [HttpGet("stats")]
        public async Task<IActionResult> GetImageStats()
        {
            try
            {
                var totalCount = await _imageRepository.GetTotalImagesCountAsync();
                var recentImages = await _imageRepository.GetRecentImagesAsync(5);

                return Ok(new
                {
                    totalImages = totalCount,
                    recentImages = recentImages.Select(img => new
                    {
                        imageId = img.Id,
                        imagePath = img.ImagePath,
                        metadata = img.Metadata,
                        createdAt = img.CreatedAt
                    }).ToList()
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting image stats");
                return StatusCode(500, new { error = "حدث خطأ أثناء جلب الإحصائيات" });
            }
        }

        [HttpGet("search-by-metadata")]
        public async Task<IActionResult> SearchByMetadata([FromQuery] string metadata)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(metadata))
                {
                    return BadRequest(new { error = "يجب تحديد البيانات الوصفية للبحث" });
                }

                var images = await _imageRepository.GetImagesByMetadataAsync(metadata);

                return Ok(new
                {
                    resultsCount = images.Count,
                    results = images.Select(img => new
                    {
                        imageId = img.Id,
                        imagePath = img.ImagePath,
                        metadata = img.Metadata,
                        createdAt = img.CreatedAt
                    }).ToList()
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching by metadata");
                return StatusCode(500, new { error = "حدث خطأ أثناء البحث" });
            }
        }

        #region Private Methods

        private IActionResult ValidateImageFile(IFormFile image, string errorMessage = "لم يتم رفع صورة صحيحة")
        {
            if (image == null || image.Length == 0)
            {
                return BadRequest(new { error = errorMessage });
            }

            // Check file type
            var allowedTypes = new[] { "image/jpeg", "image/jpg", "image/png", "image/gif", "image/bmp", "image/webp" };
            if (!allowedTypes.Contains(image.ContentType.ToLower()))
            {
                return BadRequest(new { error = "نوع الملف غير مدعوم. يرجى رفع صورة بصيغة JPG, PNG, GIF, BMP أو WebP" });
            }

            // Check file size (10MB max)
            if (image.Length > 10 * 1024 * 1024)
            {
                return BadRequest(new { error = "حجم الصورة كبير جداً. الحد الأقصى 10MB" });
            }

            return null;
        }

        private string GetSimilarityInterpretation(double similarity)
        {
            return similarity switch
            {
                >= 0.95 => "متطابقة تماماً",
                >= 0.9 => "متشابهة جداً",
                >= 0.8 => "متشابهة بدرجة عالية",
                >= 0.7 => "متشابهة",
                >= 0.6 => "متشابهة نوعاً ما",
                >= 0.4 => "متشابهة قليلاً",
                >= 0.2 => "قليلة التشابه",
                _ => "مختلفة تماماً"
            };
        }

        private string ExtractFileNameFromUrl(string cloudinaryUrl)
        {
            try
            {
                if (string.IsNullOrEmpty(cloudinaryUrl))
                    return string.Empty;

                var uri = new Uri(cloudinaryUrl);
                var segments = uri.LocalPath.Split('/');

                // Find the segment after 'upload' or 'image/upload'
                for (int i = 0; i < segments.Length; i++)
                {
                    if (segments[i] == "upload" && i + 1 < segments.Length)
                    {
                        // Skip version if present (v1234567890)
                        var nextSegment = segments[i + 1];
                        if (nextSegment.StartsWith("v") && nextSegment.Length > 1 && char.IsDigit(nextSegment[1]))
                        {
                            return i + 2 < segments.Length ? Path.GetFileNameWithoutExtension(segments[i + 2]) : string.Empty;
                        }
                        else
                        {
                            return Path.GetFileNameWithoutExtension(nextSegment);
                        }
                    }
                }

                // Fallback: just get the filename from the URL
                return Path.GetFileNameWithoutExtension(uri.LocalPath);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, $"Failed to extract filename from URL: {cloudinaryUrl}");
                return string.Empty;
            }
        }

        #endregion
    }
}
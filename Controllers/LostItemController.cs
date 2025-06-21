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
using WaslAlkhair.Api.Models;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using WaslAlkhair.Api.DTOs.Opportunity;
using Swashbuckle.AspNetCore.Annotations;

namespace WaslAlkhair.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LostItemController : ControllerBase
    {
        private readonly ILostItemService _lostItemService;
        private readonly ILostItemRepository _itemRepository;
        private readonly IFileService _cloudinaryService;
        private readonly ILogger<LostItemController> _logger;

        public LostItemController(
            ILostItemService lostItemService,
            ILostItemRepository itemRepository,
            IFileService cloudinaryService,
            ILogger<LostItemController> logger)
        {
            _lostItemService = lostItemService;
            _itemRepository = itemRepository;
            _cloudinaryService = cloudinaryService;
            _logger = logger;
        }

        [HttpPost("create")]
        [Authorize]
        [SwaggerOperation(Summary = "CREATE Lost or Found Item (must be Authorized)", Description ="Type: 0=>lost , 1=>Found ")]
        public async Task<IActionResult> CreateLostItem(
            IFormFile image,
            [FromForm] string metadata,
            [FromForm] string location,
            [FromForm] DateTime date,
            [FromForm] ItemType type,
            [FromForm] string contactInfo)
        {
            try
            {
                // Validate input
                var validationResult = ValidateImageFile(image);
                if (validationResult != null)
                    return validationResult;

                if (string.IsNullOrWhiteSpace(location))
                    return BadRequest(new { error = "يجب تحديد الموقع" });

                if (string.IsNullOrWhiteSpace(contactInfo))
                    return BadRequest(new { error = "يجب تحديد وسيلة التواصل" });

                // Get user ID from token
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(new { error = "يجب تسجيل الدخول أولاً" });

                // Create lost item with image
                var lostItem = await _lostItemService.CreateItemWithImageAsync(
                    image, metadata, location, date, type, contactInfo, userId);

                if (lostItem == null)
                {
                    return StatusCode(500, new { error = "فشل في إنشاء البلاغ" });
                }

                _logger.LogInformation($"Lost item created successfully: {lostItem.Id}");

                return Ok(new
                {
                    itemId = lostItem.Id,
                    imagePath = lostItem.ImagePath,
                    type = lostItem.Type.ToString(),
                    location = lostItem.Location,
                    date = lostItem.Date,
                    isResolved = lostItem.IsResolved,
                    message = "تم إنشاء البلاغ بنجاح"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating lost item");
                return StatusCode(500, new { error = "حدث خطأ أثناء إنشاء البلاغ" });
            }
        }

        [HttpPost("search-by-image")]
        [SwaggerOperation(Summary = "SEARCH in lost Items & found Items By Image")]
        public async Task<IActionResult> SearchByImage(IFormFile queryImage)
        {
            try
            {
                var validationResult = ValidateImageFile(queryImage, "لم يتم رفع صورة للبحث");
                if (validationResult != null)
                    return validationResult;

                using var stream = new MemoryStream();
                await queryImage.CopyToAsync(stream);
                var imageData = stream.ToArray();

                var queryEmbedding = await _lostItemService.GenerateImageEmbeddingAsync(imageData);

                if (queryEmbedding == null)
                {
                    return StatusCode(500, new { error = "فشل في معالجة صورة البحث" });
                }

                var similarItems = await _lostItemService.SearchSimilarItemsAsync(queryEmbedding);

                _logger.LogInformation($"Found {similarItems.Count} similar items");

                return Ok(new
                {
                    resultsCount = similarItems.Count,
                    results = similarItems.Select(r => new
                    {
                        itemId = r.ItemId,
                        imagePath = r.ImagePath,
                        similarity = Math.Round(r.Similarity, 4),
                        similarityPercentage = Math.Round(r.Similarity * 100, 2),
                        metadata = r.Metadata,
                        location = r.Location,
                        date = r.Date,
                        type = r.Type.ToString(),
                        contactInfo = r.ContactInfo,
                        isResolved = r.IsResolved,
                        User= new UserDto
                        {
                            Id=r.User.Id,
                            FullName=r.User.FullName,
                            image=r.User.Image,

                        }
                       
                    }).ToList()
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching by image");
                return StatusCode(500, new { error = "حدث خطأ أثناء البحث" });
            }
        }

        [HttpGet("search-by-metadata")]
        [SwaggerOperation(Summary = "SEARCH in lost Items & found Items By Metadata(description)")]
        public async Task<IActionResult> SearchByMetadata([FromQuery] string metadata)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(metadata))
                {
                    return BadRequest(new { error = "يجب تحديد البيانات الوصفية للبحث" });
                }
                
                var items = await _itemRepository.GetItemsByMetadataAsync(metadata);

                return Ok(new
                {
                    resultsCount = items.Count,
                    results = items.Select(item => new
                    {
                        itemId = item.Id,
                        imagePath = item.ImagePath,
                        metadata = item.Metadata,
                        location = item.Location,
                        date = item.Date,
                        type = item.Type.ToString(),
                        contactInfo = item.ContactInfo,
                        isResolved = item.IsResolved,
                        createdAt = item.CreatedAt,
                        User = new UserDto
                        {
                            Id = item.User.Id,
                            FullName = item.User.FullName,
                            image = item.User.image

                        }
                    }).ToList()
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching by metadata");
                return StatusCode(500, new { error = "حدث خطأ أثناء البحث" });
            }
        }

        [HttpGet("search-by-location")]
        [SwaggerOperation(Summary = "SEARCH in lost Items & found Items By Location")]
        public async Task<IActionResult> SearchByLocation([FromQuery] string location)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(location))
                {
                    return BadRequest(new { error = "يجب تحديد الموقع للبحث" });
                }

                var items = await _itemRepository.GetItemsByLocationAsync(location);

                return Ok(new
                {
                    resultsCount = items.Count,
                    results = items.Select(item => new
                    {
                        itemId = item.Id,
                        imagePath = item.ImagePath,
                        metadata = item.Metadata,
                        location = item.Location,
                        date = item.Date,
                        type = item.Type.ToString(),
                        contactInfo = item.ContactInfo,
                        isResolved = item.IsResolved,
                        createdAt = item.CreatedAt,
                        User = new UserDto
                        {
                            Id = item.User.Id,
                            FullName = item.User.FullName,
                            image = item.User.image

                        }
                    }).ToList()
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching by location");
                return StatusCode(500, new { error = "حدث خطأ أثناء البحث" });
            }
        }

        [HttpGet("by-type/{type}")]
        [SwaggerOperation(Summary = "GET all lost Items (Type=0) , OR , Get all Found Items (Type=1) ")]
        public async Task<IActionResult> GetItemsByType(ItemType type)
        {
            try
            {
                var items = await _itemRepository.GetItemsByTypeAsync(type);

                return Ok(new
                {
                    resultsCount = items.Count,
                    type = type.ToString(),
                    results = items.Select(item => new
                    {
                        itemId = item.Id,
                        imagePath = item.ImagePath,
                        metadata = item.Metadata,
                        location = item.Location,
                        date = item.Date,
                        contactInfo = item.ContactInfo,
                        isResolved = item.IsResolved,
                        createdAt = item.CreatedAt,
                        User = new UserDto
                        {
                            Id = item.User.Id,
                            FullName = item.User.FullName,
                            image = item.User.image

                        }
                    }).ToList()
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting items by type");
                return StatusCode(500, new { error = "حدث خطأ أثناء جلب البيانات" });
            }
        }

       

        [HttpGet("my-items")]
        [Authorize]
        [SwaggerOperation(Summary = "GET all the USER items (must be Authenticated) ")]
        public async Task<IActionResult> GetMyItems()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(new { error = "يجب تسجيل الدخول أولاً" });

                var items = await _itemRepository.GetItemsByUserIdAsync(userId);

                return Ok(new
                {
                    resultsCount = items.Count,
                    results = items.Select(item => new
                    {
                        itemId = item.Id,
                        imagePath = item.ImagePath,
                        metadata = item.Metadata,
                        location = item.Location,
                        date = item.Date,
                        type = item.Type.ToString(),
                        contactInfo = item.ContactInfo,
                        isResolved = item.IsResolved,
                        createdAt = item.CreatedAt
                    }).ToList()
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user items");
                return StatusCode(500, new { error = "حدث خطأ أثناء جلب البيانات" });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetItemById(Guid id)
        {
            try
            {
                var item = await _itemRepository.GetItemByIdAsync(id);
                if (item == null)
                    return NotFound(new { error = "لم يتم العثور على البلاغ" });

                return Ok(new
                {
                    itemId = item.Id,
                    imagePath = item.ImagePath,
                    metadata = item.Metadata,
                    location = item.Location,
                    date = item.Date,
                    type = item.Type.ToString(),
                    contactInfo = item.ContactInfo,
                    isResolved = item.IsResolved,
                    createdAt = item.CreatedAt,
                    userId = item.UserId
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting item by ID: {id}");
                return StatusCode(500, new { error = "حدث خطأ أثناء جلب البيانات" });
            }
        }

        [HttpPut("{id}/mark-resolved")]
        [Authorize]
        [SwaggerOperation(Summary = "The User how created the item mark it as resolved (must be Authorized)")]
        public async Task<IActionResult> MarkAsResolved(Guid id)
        {
            try
            {
                var item = await _itemRepository.GetItemByIdAsync(id);
                if (item == null)
                    return NotFound(new { error = "لم يتم العثور على البلاغ" });

                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(new { error = "يجب تسجيل الدخول أولاً" });

                // Only the owner can mark as resolved
                if (item.UserId != userId)
                    return Forbid();

                var result = await _itemRepository.MarkAsResolvedAsync(id);
                if (!result)
                    return StatusCode(500, new { error = "فشل في تحديث حالة البلاغ" });

                _logger.LogInformation($"Item marked as resolved: {id} by user: {userId}");

                return Ok(new { message = "تم تحديد البلاغ كمحلول بنجاح" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error marking item as resolved: {id}");
                return StatusCode(500, new { error = "حدث خطأ أثناء تحديث البلاغ" });
            }
        }

        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteItem(Guid id)
        {
            try
            {
                var item = await _itemRepository.GetItemByIdAsync(id);
                if (item == null)
                    return NotFound(new { error = "لم يتم العثور على البلاغ" });

                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(new { error = "يجب تسجيل الدخول أولاً" });

                // Only the owner can delete
                if (item.UserId != userId)
                    return Forbid();

                // Delete image from Cloudinary first
                if (!string.IsNullOrEmpty(item.ImagePath))
                {
                    try
                    {
                        var fileName = ExtractFileNameFromUrl(item.ImagePath);
                        await _cloudinaryService.DeleteFileAsync(fileName);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, $"Failed to delete image from Cloudinary for item: {id}");
                        // Continue with database deletion even if image deletion fails
                    }
                }

                var result = await _itemRepository.DeleteItemAsync(id);
                if (!result)
                    return StatusCode(500, new { error = "فشل في حذف البلاغ" });

                _logger.LogInformation($"Item deleted: {id} by user: {userId}");

                return Ok(new { message = "تم حذف البلاغ بنجاح" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting item: {id}");
                return StatusCode(500, new { error = "حدث خطأ أثناء حذف البلاغ" });
            }
        }

        [HttpGet("statistics")]
        public async Task<IActionResult> GetStatistics()
        {
            try
            {
                var totalItems = await _itemRepository.GetTotalItemsCountAsync();
                var unresolvedItems = await _itemRepository.GetUnresolvedItemsAsync();
                var recentItems = await _itemRepository.GetRecentItemsAsync(5);

                return Ok(new
                {
                    totalItems = totalItems,
                    unresolvedCount = unresolvedItems.Count,
                    resolvedCount = totalItems - unresolvedItems.Count,
                    recentItems = recentItems.Select(item => new
                    {
                        itemId = item.Id,
                        imagePath = item.ImagePath,
                        metadata = item.Metadata,
                        location = item.Location,
                        date = item.Date,
                        type = item.Type.ToString(),
                        isResolved = item.IsResolved,
                        createdAt = item.CreatedAt
                    }).ToList()
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting statistics");
                return StatusCode(500, new { error = "حدث خطأ أثناء جلب الإحصائيات" });
            }
        }

        #region Private Helper Methods

        private IActionResult ValidateImageFile(IFormFile image, string customErrorMessage = null)
        {
            if (image == null || image.Length == 0)
                return BadRequest(new { error = customErrorMessage ?? "يجب رفع صورة" });

            // Check file size (max 10MB)
            if (image.Length > 10 * 1024 * 1024)
                return BadRequest(new { error = "حجم الصورة يجب أن يكون أقل من 10 ميجابايت" });

            // Check file extension
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp" };
            var fileExtension = Path.GetExtension(image.FileName?.ToLowerInvariant());

            if (string.IsNullOrEmpty(fileExtension) || !allowedExtensions.Contains(fileExtension))
                return BadRequest(new { error = "نوع الملف غير مدعوم. يُسمح بـ: JPG, PNG, GIF, BMP, WEBP" });

            // Check content type
            var allowedContentTypes = new[]
            {
                "image/jpeg", "image/jpg", "image/png", "image/gif",
                "image/bmp", "image/webp"
            };

            if (!allowedContentTypes.Contains(image.ContentType?.ToLowerInvariant()))
                return BadRequest(new { error = "نوع المحتوى غير صالح" });

            return null;
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

        #endregion
    }
}
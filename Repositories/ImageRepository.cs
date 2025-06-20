using WaslAlkhair.Api.Data;
using WaslAlkhair.Api.Repositories.Interfaces;
using WaslAlkhair.Api.Services;
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
using WaslAlkhair.Api.DTOs.LostANDfound;
using WaslAlkhair.Api.Models;

namespace WaslAlkhair.Api.Repositories
{
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

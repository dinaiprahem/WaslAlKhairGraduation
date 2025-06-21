using WaslAlkhair.Api.Data;
using WaslAlkhair.Api.Repositories.Interfaces;
using WaslAlkhair.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace WaslAlkhair.Api.Repositories
{
    public class LostItemRepository : ILostItemRepository
    {
        private readonly AppDbContext _context;
        private readonly ILogger<LostItemRepository> _logger;

        public LostItemRepository(AppDbContext context, ILogger<LostItemRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<List<LostItem>> GetAllItemsWithEmbeddingsAsync()
        {
            try
            {
                return await _context.LostItems
                    .Include(x => x.User)
                    .Where(x=>x.IsResolved==false)
                    .OrderByDescending(x => x.CreatedAt)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving lost items with embeddings");
                return new List<LostItem>();
            }
        }

        public async Task SaveItemWithEmbeddingAsync(LostItem item)
        {
            try
            {
                _context.LostItems.Add(item);
                await _context.SaveChangesAsync();
                _logger.LogInformation($"Successfully saved lost item: {item.Id}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error saving lost item: {item.Id}");
                throw;
            }
        }

        public async Task<LostItem> GetItemByIdAsync(Guid id)
        {
            try
            {
                return await _context.LostItems
                    .Include(x => x.User)
                    .FirstOrDefaultAsync(x => x.Id == id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving lost item by ID: {id}");
                return null;
            }
        }

        public async Task<bool> DeleteItemAsync(Guid id)
        {
            try
            {
                var item = await _context.LostItems.FindAsync(id);
                if (item == null)
                    return false;

                _context.LostItems.Remove(item);
                var result = await _context.SaveChangesAsync();

                _logger.LogInformation($"Deleted lost item: {id}");
                return result > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting lost item: {id}");
                return false;
            }
        }

        public async Task<bool> MarkAsResolvedAsync(Guid id)
        {
            try
            {
                var item = await _context.LostItems.FindAsync(id);
                if (item == null)
                    return false;

                item.IsResolved = true;
                var result = await _context.SaveChangesAsync();

                _logger.LogInformation($"Marked lost item as resolved: {id}");
                return result > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error marking lost item as resolved: {id}");
                return false;
            }
        }

        public async Task<List<LostItem>> GetItemsByMetadataAsync(string metadata)
        {
            try
            {
                return await _context.LostItems
                    .Include(x => x.User)
                    .Where(x => x.Metadata.Contains(metadata))
                    .Where(x => x.IsResolved == false)
                    .OrderByDescending(x => x.CreatedAt)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving lost items by metadata");
                return new List<LostItem>();
            }
        }

        public async Task<List<LostItem>> GetItemsByLocationAsync(string location)
        {
            try
            {
                return await _context.LostItems
                    .Include(x => x.User)
                    .Where(x => x.IsResolved == false)
                    .Where(x => x.Location.Contains(location))
                    .OrderByDescending(x => x.CreatedAt)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving lost items by location");
                return new List<LostItem>();
            }
        }

        public async Task<List<LostItem>> GetItemsByTypeAsync(ItemType type)
        {
            try
            {
                return await _context.LostItems
                    .Include(x => x.User)
                    .Where(x=>x.IsResolved == false)
                    .Where(x => x.Type == type)
                    .OrderByDescending(x => x.CreatedAt)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving lost items by type");
                return new List<LostItem>();
            }
        }

        public async Task<List<LostItem>> GetItemsByUserIdAsync(string userId)
        {
            try
            {
                return await _context.LostItems
                    .Include(x => x.User)
                    .Where(x => x.UserId == userId)
                    .OrderByDescending(x => x.CreatedAt)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving lost items by user ID");
                return new List<LostItem>();
            }
        }

        public async Task<List<LostItem>> GetUnresolvedItemsAsync()
        {
            try
            {
                return await _context.LostItems
                    .Include(x => x.User)
                    .Where(x => !x.IsResolved)
                    .OrderByDescending(x => x.CreatedAt)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving unresolved lost items");
                return new List<LostItem>();
            }
        }

        public async Task<int> GetTotalItemsCountAsync()
        {
            try
            {
                return await _context.LostItems.CountAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting total items count");
                return 0;
            }
        }

        public async Task<List<LostItem>> GetRecentItemsAsync(int count = 10)
        {
            try
            {
                return await _context.LostItems
                    .Include(x => x.User)
                    .OrderByDescending(x => x.CreatedAt)
                    .Take(count)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving recent lost items");
                return new List<LostItem>();
            }
        }
    }
}
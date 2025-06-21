using WaslAlkhair.Api.Models;

namespace WaslAlkhair.Api.Repositories.Interfaces
{
    public interface ILostItemRepository
    {
        Task<List<LostItem>> GetAllItemsWithEmbeddingsAsync();
        Task SaveItemWithEmbeddingAsync(LostItem item);
        Task<LostItem> GetItemByIdAsync(Guid id);
        Task<bool> DeleteItemAsync(Guid id);
        Task<bool> MarkAsResolvedAsync(Guid id);
        Task<List<LostItem>> GetItemsByMetadataAsync(string metadata);
        Task<List<LostItem>> GetItemsByLocationAsync(string location);
        Task<List<LostItem>> GetItemsByTypeAsync(ItemType type);
        Task<List<LostItem>> GetItemsByUserIdAsync(string userId);
        Task<List<LostItem>> GetUnresolvedItemsAsync();
        Task<int> GetTotalItemsCountAsync();
        Task<List<LostItem>> GetRecentItemsAsync(int count = 10);
    }
}
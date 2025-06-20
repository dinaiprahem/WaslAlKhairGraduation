using WaslAlkhair.Api.DTOs.LostANDfound;
using WaslAlkhair.Api.Services;

namespace WaslAlkhair.Api.Repositories.Interfaces
{
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
}

using WaslAlkhair.Api.Models;
using Microsoft.AspNetCore.Http;
using WaslAlkhair.Api.DTOs.LostANDfound;

namespace WaslAlkhair.Api.Services
{
    public interface ILostItemService
    {
        Task<List<float>> GenerateImageEmbeddingAsync(byte[] imageData);
        Task<List<LostItemSearchResult>> SearchSimilarItemsAsync(List<float> queryEmbedding);
        Task<double> CalculateSimilarityAsync(List<float> features1, List<float> features2);
        Task<LostItem> CreateItemWithImageAsync(IFormFile image, string metadata, string location, DateTime date, ItemType type, string contactInfo, string userId);
    }

    
}
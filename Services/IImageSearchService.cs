using WaslAlkhair.Api.DTOs.LostANDfound;

namespace WaslAlkhair.Api.Services
{
    public interface IImageSearchService
    {
        Task<List<float>> GenerateImageEmbeddingAsync(byte[] imageData);
        Task<List<SearchResult>> SearchSimilarImagesAsync(List<float> queryEmbedding);
        Task<double> CalculateSimilarityAsync(List<float> features1, List<float> features2);
        Task<StoredImage> UploadAndProcessImageAsync(IFormFile image, string metadata = null);
    }
}

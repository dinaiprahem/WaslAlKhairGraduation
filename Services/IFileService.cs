namespace WaslAlkhair.Api.Services
{
    public interface IFileService
    {
        Task<string?> UploadFileAsync(IFormFile file);
        Task<bool> DeleteFileAsync(string fileName);
    }
}

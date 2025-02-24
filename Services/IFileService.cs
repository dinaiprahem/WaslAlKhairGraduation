namespace WaslAlkhair.Api.Services
{
    public interface IFileService
    {
        Task<string?> UploadFileAsync(IFormFile file, string folderName);
        Task<bool> DeleteFileAsync(string fileName, string folderName);
    }
}

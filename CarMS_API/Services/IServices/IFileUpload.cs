namespace CarMS_API.Services.IServices
{
    public interface IFileUpload
    {
        Task<string> UploadFile(IFormFile file, string subFolder);
        bool DeleteFile(string filePath);
    }
}

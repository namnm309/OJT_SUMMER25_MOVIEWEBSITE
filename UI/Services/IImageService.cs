namespace UI.Services
{
    // Services/IImageService.cs
    public interface IImageService
    {
        Task<string> UploadImageAsync(IFormFile file);
    }
}

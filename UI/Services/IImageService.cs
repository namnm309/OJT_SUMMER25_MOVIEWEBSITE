namespace UI.Services
{

    public interface IImageService
    {
        Task<string> UploadImageAsync(IFormFile file);
        Task<string> UploadVideoAsync(IFormFile file);
    }
}

using CloudinaryDotNet.Actions;
using CloudinaryDotNet;

namespace UI.Services
{
    public class CloudinaryImageService : IImageService
    {
        private readonly Cloudinary _cloudinary;
        private readonly ILogger<CloudinaryImageService> _logger;

        public CloudinaryImageService(IConfiguration configuration, ILogger<CloudinaryImageService> logger)
        {
            var cloudName = configuration["Cloudinary:CloudName"];
            var apiKey = configuration["Cloudinary:ApiKey"];
            var apiSecret = configuration["Cloudinary:ApiSecret"];

            Account account = new Account(cloudName, apiKey, apiSecret);
            _cloudinary = new Cloudinary(account);
            _logger = logger;
        }

        public async Task<string> UploadImageAsync(IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                    throw new ArgumentException("File is empty");

                await using var stream = file.OpenReadStream();
                var uploadParams = new ImageUploadParams()
                {
                    File = new FileDescription(file.FileName, stream),
                    PublicId = $"promotions/{Guid.NewGuid()}",
                    Overwrite = false,
                    Transformation = new Transformation().Width(800).Height(600).Crop("limit")
                };

                var uploadResult = await _cloudinary.UploadAsync(uploadParams);

                if (uploadResult.Error != null)
                {
                    _logger.LogError("Cloudinary upload error: {Error}", uploadResult.Error.Message);
                    throw new Exception(uploadResult.Error.Message);
                }

                _logger.LogInformation("Image uploaded successfully: {Url}", uploadResult.SecureUrl.ToString());
                return uploadResult.SecureUrl.ToString();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading image to Cloudinary");
                throw;
            }
        }
    }
}

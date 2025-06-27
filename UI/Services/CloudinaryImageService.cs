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
            _logger = logger;
            
            var cloudName = configuration["Cloudinary:CloudName"];
            var apiKey = configuration["Cloudinary:ApiKey"];
            var apiSecret = configuration["Cloudinary:ApiSecret"];

            // Check if Cloudinary is properly configured
            if (string.IsNullOrEmpty(cloudName) || string.IsNullOrEmpty(apiKey) || string.IsNullOrEmpty(apiSecret) ||
                apiKey == "your-api-key-here" || apiSecret == "your-api-secret-here")
            {
                _logger.LogWarning("⚠️ Cloudinary not configured - using mock mode");
                _cloudinary = null; // Will use mock responses
            }
            else
            {
                try
                {
            Account account = new Account(cloudName, apiKey, apiSecret);
            _cloudinary = new Cloudinary(account);
        }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "❌ Failed to initialize Cloudinary - falling back to mock mode");
                    _cloudinary = null;
                }
            }
        }

        public async Task<string> UploadImageAsync(IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                    throw new ArgumentException("File is empty");

                // Mock mode - return placeholder URL
                if (_cloudinary == null)
                {
                    var mockUrl = $"https://via.placeholder.com/800x600/1a1a1a/ffffff?text={Uri.EscapeDataString(file.FileName)}";
                    _logger.LogWarning("🎭 MOCK MODE: Returning placeholder URL: {Url}", mockUrl);
                    await Task.Delay(500); // Simulate upload delay
                    return mockUrl;
                }

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

        public async Task<string> UploadVideoAsync(IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                    throw new ArgumentException("File is empty");

                // Mock mode - return placeholder video URL
                if (_cloudinary == null)
                {
                    var mockUrl = $"https://sample-videos.com/zip/10/mp4/SampleVideo_1280x720_1mb.mp4?mock={file.FileName}";
                    _logger.LogWarning("🎭 MOCK MODE: Returning placeholder video URL: {Url}", mockUrl);
                    await Task.Delay(1000); // Simulate video upload delay
                    return mockUrl;
                }

                await using var stream = file.OpenReadStream();
                var uploadParams = new VideoUploadParams()
                {
                    File = new FileDescription(file.FileName, stream),
                    PublicId = $"videos/{Guid.NewGuid()}",
                    Overwrite = false,
                };

                var uploadResult = await _cloudinary.UploadAsync(uploadParams);

                if (uploadResult.Error != null)
                {
                    _logger.LogError("Cloudinary upload error: {Error}", uploadResult.Error.Message);
                    throw new Exception(uploadResult.Error.Message);
                }

                _logger.LogInformation("Video uploaded successfully: {Url}", uploadResult.SecureUrl.ToString());
                return uploadResult.SecureUrl.ToString();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading video to Cloudinary");
                throw;
            }
        }
    }
}

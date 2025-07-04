using UI.Models;

namespace UI.Services
{
    public interface IAuthUIService
    {
        Task<ApiResponse<dynamic>> LoginAsync(LoginViewModel model);
        Task<ApiResponse<dynamic>> RegisterAsync(RegisterViewModel model);
        Task<ApiResponse<dynamic>> LogoutAsync();
        Task<ApiResponse<dynamic>> GetCurrentUserAsync();
    }

    public class AuthUIService : IAuthUIService
    {
        private readonly IApiService _apiService;
        private readonly ILogger<AuthUIService> _logger;

        public AuthUIService(IApiService apiService, ILogger<AuthUIService> logger)
        {
            _apiService = apiService;
            _logger = logger;
        }

        public async Task<ApiResponse<dynamic>> LoginAsync(LoginViewModel model)
        {
            try
            {
                _logger.LogInformation("Attempting user login for: {Username}", model.Username);
                
                var response = await _apiService.PostAsync<dynamic>("user/login", model);
                
                if (response.Success)
                {
                    _logger.LogInformation("Login successful for user: {Username}", model.Username);
                }
                else
                {
                    _logger.LogWarning("Login failed for user: {Username}. Reason: {Message}", 
                        model.Username, response.Message);
                }
                
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login for user: {Username}", model.Username);
                return new ApiResponse<dynamic>
                {
                    Success = false,
                    Message = "Đã xảy ra lỗi trong quá trình đăng nhập. Vui lòng thử lại."
                };
            }
        }

        public async Task<ApiResponse<dynamic>> RegisterAsync(RegisterViewModel model)
        {
            try
            {
                _logger.LogInformation("Attempting user registration for: {Username}", model.Username);
                
                var response = await _apiService.PostAsync<dynamic>("user/register", model);
                
                if (response.Success)
                {
                    _logger.LogInformation("Registration successful for user: {Username}", model.Username);
                }
                else
                {
                    _logger.LogWarning("Registration failed for user: {Username}. Reason: {Message}", 
                        model.Username, response.Message);
                }
                
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during registration for user: {Username}", model.Username);
                return new ApiResponse<dynamic>
                {
                    Success = false,
                    Message = "Đã xảy ra lỗi trong quá trình đăng ký. Vui lòng thử lại."
                };
            }
        }

        public async Task<ApiResponse<dynamic>> LogoutAsync()
        {
            try
            {
                _logger.LogInformation("User logout attempt");
                
                var response = await _apiService.PostAsync<dynamic>("user/logout", null);
                
                _logger.LogInformation("Logout completed");
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during logout");
                return new ApiResponse<dynamic>
                {
                    Success = false,
                    Message = "Đã xảy ra lỗi trong quá trình đăng xuất."
                };
            }
        }

        public async Task<ApiResponse<dynamic>> GetCurrentUserAsync()
        {
            try
            {
                _logger.LogInformation("Getting current user information");
                
                var response = await _apiService.GetAsync<dynamic>("user/current");
                
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting current user information");
                return new ApiResponse<dynamic>
                {
                    Success = false,
                    Message = "Không thể lấy thông tin người dùng hiện tại."
                };
            }
        }
    }
} 
using UI.Models;
using UI.Areas.UserManagement.Models;
using UI.Services;

namespace UI.Areas.UserManagement.Services
{
    public interface IUserManagementUIService
    {
        // T1: Login
        Task<ApiResponse<dynamic>> LoginAsync(LoginViewModel model);
        
        // T2: Register
        Task<ApiResponse<dynamic>> RegisterAsync(RegisterViewModel model);
        
        // T3: Edit Profile
        Task<ApiResponse<dynamic>> GetProfileAsync();
        Task<ApiResponse<dynamic>> UpdateProfileAsync(UserProfileViewModel model);
        Task<ApiResponse<dynamic>> ChangePasswordAsync(ChangePasswordViewModel model);
        
        // T4: View List Members (Admin)
        Task<ApiResponse<dynamic>> GetMembersAsync();
        
        // T5: View Booked Tickets
        Task<ApiResponse<dynamic>> GetBookedTicketsAsync();
        Task<ApiResponse<dynamic>> CancelTicketAsync(Guid bookingId);
        
        // T6: View History of Score
        Task<ApiResponse<dynamic>> GetScoreHistoryAsync(DateTime? fromDate = null, DateTime? toDate = null, string historyType = "adding");
        
        // Common
        Task<ApiResponse<dynamic>> LogoutAsync();
        Task<ApiResponse<dynamic>> GetCurrentUserAsync();
    }

    public class UserManagementUIService : IUserManagementUIService
    {
        private readonly IApiService _apiService;
        private readonly ILogger<UserManagementUIService> _logger;

        public UserManagementUIService(IApiService apiService, ILogger<UserManagementUIService> logger)
        {
            _apiService = apiService;
            _logger = logger;
        }

        public async Task<ApiResponse<dynamic>> LoginAsync(LoginViewModel model)
        {
            try
            {
                _logger.LogInformation("User login attempt for: {Username}", model.Username);
                return await _apiService.PostAsync<dynamic>("user/login", model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login");
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
                _logger.LogInformation("User registration attempt for: {Username}", model.Username);
                return await _apiService.PostAsync<dynamic>("user/register", model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during registration");
                return new ApiResponse<dynamic>
                {
                    Success = false,
                    Message = "Đã xảy ra lỗi trong quá trình đăng ký. Vui lòng thử lại."
                };
            }
        }

        public async Task<ApiResponse<dynamic>> GetProfileAsync()
        {
            try
            {
                _logger.LogInformation("Getting user profile");
                return await _apiService.GetAsync<dynamic>("user/profile");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user profile");
                return new ApiResponse<dynamic>
                {
                    Success = false,
                    Message = "Không thể tải thông tin cá nhân. Vui lòng thử lại."
                };
            }
        }

        public async Task<ApiResponse<dynamic>> UpdateProfileAsync(UserProfileViewModel model)
        {
            try
            {
                _logger.LogInformation("Updating user profile for: {Username}", model.Username);
                return await _apiService.PutAsync<dynamic>("user/profile", model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user profile");
                return new ApiResponse<dynamic>
                {
                    Success = false,
                    Message = "Không thể cập nhật thông tin cá nhân. Vui lòng thử lại."
                };
            }
        }

        public async Task<ApiResponse<dynamic>> ChangePasswordAsync(ChangePasswordViewModel model)
        {
            try
            {
                _logger.LogInformation("Changing user password");
                return await _apiService.PutAsync<dynamic>("user/change-password", model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error changing password");
                return new ApiResponse<dynamic>
                {
                    Success = false,
                    Message = "Không thể thay đổi mật khẩu. Vui lòng thử lại."
                };
            }
        }

        public async Task<ApiResponse<dynamic>> GetMembersAsync()
        {
            try
            {
                _logger.LogInformation("Getting members list");
                return await _apiService.GetAsync<dynamic>("admin/members");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting members list");
                return new ApiResponse<dynamic>
                {
                    Success = false,
                    Message = "Không thể tải danh sách thành viên. Vui lòng thử lại."
                };
            }
        }

        public async Task<ApiResponse<dynamic>> GetBookedTicketsAsync()
        {
            try
            {
                _logger.LogInformation("Getting user booked tickets");
                return await _apiService.GetAsync<dynamic>("user/booked-tickets");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting booked tickets");
                return new ApiResponse<dynamic>
                {
                    Success = false,
                    Message = "Không thể tải danh sách vé đã đặt. Vui lòng thử lại."
                };
            }
        }

        public async Task<ApiResponse<dynamic>> CancelTicketAsync(Guid bookingId)
        {
            try
            {
                _logger.LogInformation("Cancelling ticket for booking: {BookingId}", bookingId);
                return await _apiService.PutAsync<dynamic>($"booking/{bookingId}/cancel", null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling ticket");
                return new ApiResponse<dynamic>
                {
                    Success = false,
                    Message = "Không thể hủy vé. Vui lòng thử lại."
                };
            }
        }

        public async Task<ApiResponse<dynamic>> GetScoreHistoryAsync(DateTime? fromDate = null, DateTime? toDate = null, string historyType = "adding")
        {
            try
            {
                _logger.LogInformation("Getting score history");
                
                var queryParams = new List<string>();
                if (fromDate.HasValue) queryParams.Add($"fromDate={fromDate.Value:yyyy-MM-dd}");
                if (toDate.HasValue) queryParams.Add($"toDate={toDate.Value:yyyy-MM-dd}");
                queryParams.Add($"type={Uri.EscapeDataString(historyType)}");
                
                var queryString = string.Join("&", queryParams);
                return await _apiService.GetAsync<dynamic>($"user/score-history?{queryString}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting score history");
                return new ApiResponse<dynamic>
                {
                    Success = false,
                    Message = "Không thể tải lịch sử điểm thưởng. Vui lòng thử lại."
                };
            }
        }

        public async Task<ApiResponse<dynamic>> LogoutAsync()
        {
            try
            {
                _logger.LogInformation("User logout");
                return await _apiService.PostAsync<dynamic>("user/logout", null);
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
                return await _apiService.GetAsync<dynamic>("user/current");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting current user");
                return new ApiResponse<dynamic>
                {
                    Success = false,
                    Message = "Không thể lấy thông tin người dùng hiện tại."
                };
            }
        }
    }
} 
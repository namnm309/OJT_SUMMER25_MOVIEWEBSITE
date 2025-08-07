using ApplicationLayer.DTO.UserManagement;

namespace ApplicationLayer.Services.UserManagement
{
    public interface IUserService
    {
        Task<(bool Success, UserResponseDto? User, string Message)> LoginAsync(LoginRequestDto loginRequest);
        Task<(bool Success, string Message)> RegisterAsync(RegisterRequestDto registerRequest);
        Task<(bool Success, UserResponseDto? User, string Message)> EditProfileAsync(Guid userId, EditProfileRequestDto editRequest);
        Task<List<UserResponseDto>> GetAllMembersAsync();
        Task<UserResponseDto?> GetUserByIdAsync(Guid userId);
        string HashPassword(string password);
        bool VerifyPassword(string password, string hashedPassword);
        Task<CustomerSearchDto?> SearchCustomerAsync(string searchTerm);
        
        // Validation methods
        Task<bool> IsUsernameExistsAsync(string username);
        Task<bool> IsPhoneExistsAsync(string phone);
        Task<bool> IsEmailExistsAsync(string email);
        Task<bool> IsIdentityCardExistsAsync(string identityCard);
        
        // Admin operations
        Task<(bool Success, UserResponseDto? User, string Message)> CreateUserAsync(UserCreateDto createRequest);
        Task<(bool Success, UserResponseDto? User, string Message)> UpdateUserAsync(Guid userId, UserUpdateDto updateRequest);
        Task<(bool Success, string Message)> DeleteUserAsync(Guid userId);
        Task<(bool Success, UserResponseDto? User, string Message)> ToggleUserStatusAsync(Guid userId);
        
        // Dashboard statistics
        Task<int> GetUserCountAsync();
        Task<double> GetUserGrowthAsync();
    }
}
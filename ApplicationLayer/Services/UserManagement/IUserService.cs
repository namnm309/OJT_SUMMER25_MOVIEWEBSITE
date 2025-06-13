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
    }
} 
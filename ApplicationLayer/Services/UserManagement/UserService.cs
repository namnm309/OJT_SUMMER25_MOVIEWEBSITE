using ApplicationLayer.DTO.UserManagement;
using DomainLayer.Entities;
using DomainLayer.Enum;
using InfrastructureLayer.Repository;

namespace ApplicationLayer.Services.UserManagement
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;

        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        //Login
        public async Task<(bool Success, UserResponseDto? User, string Message)> LoginAsync(LoginRequestDto loginRequest)
        {
            try
            {
                var user = await _userRepository.GetByUsernameAsync(loginRequest.Username);
                if (user == null)
                {
                    return (false, null, "Username/password is invalid. Please try again!");
                }

                if (!VerifyPassword(loginRequest.Password, user.Password))
                {
                    return (false, null, "Username/password is invalid. Please try again!");
                }

                var userResponse = MapToUserResponseDto(user);
                return (true, userResponse, "Login successful");
            }
            catch (Exception ex)
            {
                return (false, null, $"An error occurred: {ex.Message}");
            }
        }

        //Register 
        public async Task<(bool Success, string Message)> RegisterAsync(RegisterRequestDto registerRequest)
        {
            try
            {
                // Check if username exists
                if (await _userRepository.IsUsernameExistsAsync(registerRequest.Username))
                {
                    return (false, "Username already exists");
                }

                // Check if email exists
                if (await _userRepository.IsEmailExistsAsync(registerRequest.Email))
                {
                    return (false, "Email already exists");
                }

                var user = new Users
                {
                    UserId = Guid.NewGuid(),
                    Username = registerRequest.Username,
                    Password = HashPassword(registerRequest.Password),
                    Email = registerRequest.Email,
                    FullName = registerRequest.FullName,
                    Phone = registerRequest.Phone,
                    IdentityCard = registerRequest.IdentityCard,
                    Address = registerRequest.Address,
                    BirthDate = registerRequest.BirthDate,
                    Gender = registerRequest.Gender,
                    Role = UserRole.Member,
                    Score = 0.0,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                await _userRepository.CreateAsync(user);
                return (true, "Registration successful");
            }
            catch (Exception ex)
            {
                return (false, $"An error occurred: {ex.Message}");
            }
        }

        //Edit Profile
        public async Task<(bool Success, UserResponseDto? User, string Message)> EditProfileAsync(Guid userId, EditProfileRequestDto editRequest)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                {
                    return (false, null, "User not found");
                }

                // Check if email is being changed and already exists
                if (user.Email != editRequest.Email && await _userRepository.IsEmailExistsAsync(editRequest.Email))
                {
                    return (false, null, "Email already exists");
                }

                // Update user info
                user.Email = editRequest.Email;
                user.FullName = editRequest.FullName;
                user.Phone = editRequest.Phone;
                user.IdentityCard = editRequest.IdentityCard;
                user.Address = editRequest.Address;
                user.BirthDate = editRequest.BirthDate;
                user.Gender = editRequest.Gender;
                user.Avatar = editRequest.Avatar;

                // Update password if provided
                if (!string.IsNullOrEmpty(editRequest.NewPassword))
                {
                    user.Password = HashPassword(editRequest.NewPassword);
                }

                var updatedUser = await _userRepository.UpdateAsync(user);
                var userResponse = MapToUserResponseDto(updatedUser);

                return (true, userResponse, "Update information successfully");
            }
            catch (Exception ex)
            {
                return (false, null, $"An error occurred: {ex.Message}");
            }
        }

        //Xemm all user => admin
        public async Task<List<UserResponseDto>> GetAllMembersAsync()
        {
            var users = await _userRepository.GetAllMembersAsync();
            return users.Select(MapToUserResponseDto).ToList();
        }

        //Tìm user - id 
        public async Task<UserResponseDto?> GetUserByIdAsync(Guid userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            return user == null ? null : MapToUserResponseDto(user);
        }

        //Method hash password xài chung 
        public string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        //Verify password
        public bool VerifyPassword(string password, string hashedPassword)
        {
            return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
        }

        //
        private UserResponseDto MapToUserResponseDto(Users user)
        {
            return new UserResponseDto
            {
                UserId = user.UserId,
                Username = user.Username,
                Email = user.Email,
                FullName = user.FullName,
                Phone = user.Phone,
                IdentityCard = user.IdentityCard,
                Address = user.Address,
                Role = user.Role,
                Score = user.Score,
                BirthDate = user.BirthDate,
                Gender = user.Gender,
                Avatar = user.Avatar,
                CreatedAt = user.CreatedAt,
                UpdatedAt = user.UpdatedAt
            };
        }
    }
} 
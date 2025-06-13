using ApplicationLayer.DTO.UserManagement;
using DomainLayer.Entities;
using DomainLayer.Enum;
using InfrastructureLayer.Repository;
using AutoMapper;

namespace ApplicationLayer.Services.UserManagement
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper; // AutoMapper để convert objects

        public UserService(IUserRepository userRepository, IMapper mapper)
        {
            _userRepository = userRepository;
            _mapper = mapper;
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

                // Dùng AutoMapper để convert từ Users entity sang UserResponseDto
                var userResponse = _mapper.Map<UserResponseDto>(user);
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

                // Dùng AutoMapper để convert từ RegisterRequestDto sang Users entity
                var user = _mapper.Map<Users>(registerRequest);
                
                // Set những field đặc biệt (không map tự động)
                user.UserId = Guid.NewGuid();
                user.Password = HashPassword(registerRequest.Password); // Hash password

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
                // Dùng AutoMapper để convert
                var userResponse = _mapper.Map<UserResponseDto>(updatedUser);

                return (true, userResponse, "Update information successfully");
            }
            catch (Exception ex)
            {
                return (false, null, $"An error occurred: {ex.Message}");
            }
        }

        //Xem all user => admin
        public async Task<List<UserResponseDto>> GetAllMembersAsync()
        {
            var users = await _userRepository.GetAllMembersAsync();
            // Dùng AutoMapper để convert List<Users> sang List<UserResponseDto>
            return _mapper.Map<List<UserResponseDto>>(users);
        }

        //Tìm user - id 
        public async Task<UserResponseDto?> GetUserByIdAsync(Guid userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            // Dùng AutoMapper để convert, return null nếu user không tồn tại
            return user == null ? null : _mapper.Map<UserResponseDto>(user);
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

        // Method MapToUserResponseDto đã được thay thế bằng AutoMapper
        // Không cần manual mapping nữa! 🎉
    }
} 
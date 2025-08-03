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
                user.Id = Guid.NewGuid();
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

        public async Task<CustomerSearchDto?> SearchCustomerAsync(string searchTerm)
        {
            try 
            {
                // Tìm kiếm theo số điện thoại hoặc email
                var user = await _userRepository.SearchCustomerAsync(searchTerm);
                
                if (user == null) return null;

                // Tìm tổng số vé đã đặt
                var totalBookings = await _userRepository.GetTotalBookingsAsync(user.Id);
                
                // Tìm ngày đặt vé cuối cùng
                var lastBookingDate = await _userRepository.GetLastBookingDateAsync(user.Id);

                // Mapping thủ công vì AutoMapper có thể không map được hết
                return new CustomerSearchDto 
                {
                    Id = user.Id,
                    FullName = user.FullName,
                    Email = user.Email ?? string.Empty,
                    PhoneNumber = user.Phone ?? string.Empty,
                    Points = (int)Math.Round(user.Score),
                    TotalBookings = totalBookings,
                    LastBookingDate = lastBookingDate
                };
            }
            catch (Exception)
            {
                // Log lỗi nếu cần
                return null;
            }
        }

        // ============ ADMIN OPERATIONS ============

        public async Task<(bool Success, UserResponseDto? User, string Message)> CreateUserAsync(UserCreateDto createRequest)
        {
            try
            {
                // Check if username exists
                if (await _userRepository.IsUsernameExistsAsync(createRequest.Username))
                {
                    return (false, null, "Username already exists");
                }

                // Check if email exists
                if (await _userRepository.IsEmailExistsAsync(createRequest.Email))
                {
                    return (false, null, "Email already exists");
                }

                // Check if phone exists
                if (!string.IsNullOrEmpty(createRequest.Phone) && await _userRepository.IsPhoneExistsAsync(createRequest.Phone))
                {
                    return (false, null, "Phone number already exists");
                }

                // Check if identity card exists
                if (!string.IsNullOrEmpty(createRequest.IdentityCard) && await _userRepository.IsIdentityCardExistsAsync(createRequest.IdentityCard))
                {
                    return (false, null, "Identity card already exists");
                }

                // Create new user
                var user = new Users
                {
                    Id = Guid.NewGuid(),
                    Username = createRequest.Username,
                    Email = createRequest.Email,
                    FullName = createRequest.FullName,
                    Password = HashPassword(createRequest.Password),
                    Phone = createRequest.Phone,
                    Address = createRequest.Address,
                    IdentityCard = createRequest.IdentityCard,
                    Role = createRequest.Role,
                    Gender = createRequest.Gender,
                    BirthDate = createRequest.BirthDate,
                    Avatar = createRequest.Avatar,
                    Score = createRequest.Score,
                    IsActive = createRequest.IsActive,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                var createdUser = await _userRepository.CreateAsync(user);
                var userResponse = _mapper.Map<UserResponseDto>(createdUser);

                return (true, userResponse, "User created successfully");
            }
            catch (Exception ex)
            {
                return (false, null, $"An error occurred: {ex.Message}");
            }
        }

        public async Task<(bool Success, UserResponseDto? User, string Message)> UpdateUserAsync(Guid userId, UserUpdateDto updateRequest)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                {
                    return (false, null, "User not found");
                }

                // Check if email is being changed and already exists
                if (!string.IsNullOrEmpty(updateRequest.Email) && 
                    user.Email != updateRequest.Email && 
                    await _userRepository.IsEmailExistsAsync(updateRequest.Email))
                {
                    return (false, null, "Email already exists");
                }

                // Check if phone is being changed and already exists
                if (!string.IsNullOrEmpty(updateRequest.Phone) && 
                    user.Phone != updateRequest.Phone && 
                    await _userRepository.IsPhoneExistsAsync(updateRequest.Phone))
                {
                    return (false, null, "Phone number already exists");
                }

                // Check if identity card is being changed and already exists
                if (!string.IsNullOrEmpty(updateRequest.IdentityCard) && 
                    user.IdentityCard != updateRequest.IdentityCard && 
                    await _userRepository.IsIdentityCardExistsAsync(updateRequest.IdentityCard))
                {
                    return (false, null, "Identity card already exists");
                }

                // Update only provided fields (PATCH behavior)
                if (!string.IsNullOrEmpty(updateRequest.FullName))
                    user.FullName = updateRequest.FullName;
                
                if (!string.IsNullOrEmpty(updateRequest.Email))
                    user.Email = updateRequest.Email;
                
                if (updateRequest.Phone != null)
                    user.Phone = updateRequest.Phone;
                
                if (updateRequest.Address != null)
                    user.Address = updateRequest.Address;
                
                if (updateRequest.IdentityCard != null)
                    user.IdentityCard = updateRequest.IdentityCard;
                
                if (updateRequest.Role.HasValue)
                    user.Role = updateRequest.Role.Value;
                
                if (updateRequest.Gender.HasValue)
                    user.Gender = updateRequest.Gender.Value;
                
                if (updateRequest.BirthDate.HasValue)
                    user.BirthDate = updateRequest.BirthDate;
                
                if (updateRequest.Avatar != null)
                    user.Avatar = updateRequest.Avatar;
                
                if (updateRequest.Score.HasValue)
                    user.Score = updateRequest.Score.Value;
                
                if (updateRequest.IsActive.HasValue)
                    user.IsActive = updateRequest.IsActive.Value;

                // Update password if provided
                if (!string.IsNullOrEmpty(updateRequest.NewPassword))
                {
                    user.Password = HashPassword(updateRequest.NewPassword);
                }

                user.UpdatedAt = DateTime.UtcNow;

                var updatedUser = await _userRepository.UpdateAsync(user);
                var userResponse = _mapper.Map<UserResponseDto>(updatedUser);

                return (true, userResponse, "User updated successfully");
            }
            catch (Exception ex)
            {
                return (false, null, $"An error occurred: {ex.Message}");
            }
        }

        public async Task<(bool Success, string Message)> DeleteUserAsync(Guid userId)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                {
                    return (false, "User not found");
                }

                // Soft delete - just mark as inactive
                user.IsActive = false;
                user.UpdatedAt = DateTime.UtcNow;

                await _userRepository.UpdateAsync(user);

                return (true, "User deleted successfully");
            }
            catch (Exception ex)
            {
                return (false, $"An error occurred: {ex.Message}");
            }
        }

        public async Task<(bool Success, UserResponseDto? User, string Message)> ToggleUserStatusAsync(Guid userId)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                {
                    return (false, null, "User not found");
                }

                user.IsActive = !user.IsActive;
                await _userRepository.UpdateAsync(user);

                var userResponse = _mapper.Map<UserResponseDto>(user);
                return (true, userResponse, $"User {(user.IsActive ? "activated" : "deactivated")} successfully");
            }
            catch (Exception ex)
            {
                return (false, null, $"An error occurred: {ex.Message}");
            }
        }

        // Dashboard statistics
        public async Task<int> GetUserCountAsync()
        {
            try
            {
                return await _userRepository.GetUserCountAsync();
            }
            catch (Exception ex)
            {
                // Log error and return 0
                return 0;
            }
        }

        public async Task<double> GetUserGrowthAsync()
        {
            try
            {
                return await _userRepository.GetUserGrowthAsync();
            }
            catch (Exception ex)
            {
                // Log error and return 0
                return 0;
            }
        }
    }
} 
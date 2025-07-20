using DomainLayer.Enum;

namespace ApplicationLayer.DTO.UserManagement
{
    public class UserUpdateDto
    {
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? Address { get; set; }
        public string? IdentityCard { get; set; }
        public UserRole? Role { get; set; }
        public UserGender? Gender { get; set; }
        public DateTime? BirthDate { get; set; }
        public string? Avatar { get; set; }
        public double? Score { get; set; }
        public string? NewPassword { get; set; }
        public bool? IsActive { get; set; }
    }
} 
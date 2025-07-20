using DomainLayer.Enum;
using System.ComponentModel.DataAnnotations;

namespace ApplicationLayer.DTO.UserManagement
{
    public class UserCreateDto
    {
        [Required]
        public string Username { get; set; } = string.Empty;
        
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
        
        [Required]
        public string FullName { get; set; } = string.Empty;
        
        [Required]
        [MinLength(6)]
        public string Password { get; set; } = string.Empty;
        
        [Required]
        [Phone]
        public string Phone { get; set; } = string.Empty;

        [Required]
        [MaxLength(255)]
        public string Address { get; set; } = string.Empty;
        [Required]
        [MaxLength(20)]
        public string IdentityCard { get; set; } = string.Empty;
        
        [Required]
        public UserRole Role { get; set; } = UserRole.Member;
        
        public UserGender Gender { get; set; } = UserGender.Male;
        public DateTime? BirthDate { get; set; }
        public string? Avatar { get; set; }
        public double Score { get; set; } = 0.0;
        public bool IsActive { get; set; } = true;
    }
} 
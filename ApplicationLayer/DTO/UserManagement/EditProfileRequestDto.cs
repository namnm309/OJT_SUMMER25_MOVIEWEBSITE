using DomainLayer.Enum;
using System.ComponentModel.DataAnnotations;

namespace ApplicationLayer.DTO.UserManagement
{
    public class EditProfileRequestDto
    {
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        [StringLength(100, ErrorMessage = "Email cannot exceed 100 characters")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Full name is required")]
        [StringLength(100, ErrorMessage = "Full name cannot exceed 100 characters")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Phone number is required")]
        [Phone(ErrorMessage = "Invalid phone number format")]
        public string Phone { get; set; } = string.Empty;

        [StringLength(20, ErrorMessage = "Identity card cannot exceed 20 characters")]
        public string? IdentityCard { get; set; }

        [Required(ErrorMessage = "Address is required")]
        [StringLength(255, ErrorMessage = "Address cannot exceed 255 characters")]
        public string Address { get; set; } = string.Empty;

        public DateTime? BirthDate { get; set; }

        public UserGender Gender { get; set; }

        [StringLength(500, ErrorMessage = "Avatar URL cannot exceed 500 characters")]
        public string? Avatar { get; set; }

        // For password change
        [StringLength(255, MinimumLength = 8, ErrorMessage = "New password must be between 8 and 255 characters")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[!@#$%^&*()_+\-=\[\]{};':""\\|,.<>\/?~`]).{8,}$", 
            ErrorMessage = "Password must contain at least 8 characters, including uppercase, lowercase, number, and special character")]
        public string? NewPassword { get; set; }

        [Compare("NewPassword", ErrorMessage = "Password and confirm password do not match")]
        public string? ConfirmNewPassword { get; set; }
    }
}
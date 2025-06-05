using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationLayer.DTO.Auth
{
    public class Register
    {
        [Required]
        [MaxLength(50)]
        [RegularExpression(@"^\S(.*\S)?$", ErrorMessage = "Username cannot start or end with whitespace.")]
        public string Username { get; set; } = string.Empty;

        [Required]
        [MinLength(6)]
        public string Password { get; set; } = string.Empty;

        [Required]
        [Compare("Password", ErrorMessage = "Passwords do not match.")]
        public string ConfirmPassword { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        [RegularExpression(@"^\S(.*\S)?$", ErrorMessage = "Full name cannot start or end with whitespace.")]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Date)]
        public DateTime BirthDate { get; set; }

        [Required]
        [MaxLength(10)]
        public string Gender { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        [MaxLength(100, ErrorMessage = "Email must not exceed 100 characters.")]
        [RegularExpression(@"^[^\s][a-zA-Z0-9._%+-]+@gmail\.com[^\s]*$", ErrorMessage = "Only valid Gmail addresses without surrounding spaces are allowed.")]
        public string Email { get; set; } = string.Empty;

        [Required]
        [Phone]
        [RegularExpression(@"^\S(.*\S)?$", ErrorMessage = "Phone cannot start or end with whitespace.")]
        public string Phone { get; set; } = string.Empty;

        [Required]
        [MaxLength(255)]
        [RegularExpression(@"^\S(.*\S)?$", ErrorMessage = "Address cannot start or end with whitespace.")]
        public string Address { get; set; } = string.Empty;
    }
}

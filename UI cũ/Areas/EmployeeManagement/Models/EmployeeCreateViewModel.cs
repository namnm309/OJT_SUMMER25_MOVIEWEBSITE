using System.ComponentModel.DataAnnotations;

namespace UI.Areas.EmployeeManagement.Models
{
    public class EmployeeCreateViewModel
    {
        [Required]
        public string Username { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;

        [Required]
        public string FullName { get; set; } = string.Empty;

        [Required]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Phone { get; set; } = string.Empty;

        [Required]
        public string IdentityCard { get; set; } = string.Empty;

        [Required]
        public string Address { get; set; } = string.Empty;

        public DateTime? BirthDate { get; set; }

        public string? Gender { get; set; }

        public string? Position { get; set; }

        public decimal? Salary { get; set; }

        public DateTime? HireDate { get; set; }
    }

    public class EmployeeUpdateViewModel : EmployeeCreateViewModel
    {
        // Inherit all fields from Create
    }
} 
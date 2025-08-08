using DomainLayer.Enum;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationLayer.DTO.EmployeeManagement
{
    public class EmployeeCreateDto
    {
        [Required, MaxLength(28)] 
        public string Account { get; set; } = string.Empty;

        [Required, MaxLength(28)] 
        public string Password { get; set; } = string.Empty;

        [Required, MaxLength(28)] 
        public string ConfirmPassword { get; set; } = string.Empty;

        [Required, MaxLength(100)] 
        public string FullName { get; set; } = string.Empty;

        [Required, EmailAddress, MaxLength(100)] 
        public string Email { get; set; } = string.Empty;

        [Required, MaxLength(20)] 
        public string IdentityCard { get; set; } = string.Empty;

        [Required] 
        public UserGender Gender { get; set; }

        [Required, Phone, MaxLength(28)] 
        public string PhoneNumber { get; set; } = string.Empty;

        [Required, MaxLength(28)] 
        public string Address { get; set; } = string.Empty;

        [Required] 
        public DateTime DateOfBirth { get; set; }

        [MaxLength(500)] 
        public string? ProfileImageUrl { get; set; }
    }
}

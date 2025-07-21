using DomainLayer.Enum;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationLayer.DTO.JWT
{
    public class UserDto
    {
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string IdentityCard { get; set; } = string.Empty;
        public UserRole Role { get; set; } = UserRole.Member;
        public string FullName { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public double Score { get; set; }
        public DateTime? BirthDate { get; set; }
        public UserGender Gender { get; set; }
        public bool IsActive { get; set; } = true;
        public string? Avatar { get; set; }
    }
}

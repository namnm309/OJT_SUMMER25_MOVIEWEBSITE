using DomainLayer.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationLayer.DTO.JWT
{
    public class RegisterReq
    {
        public string Email { get; set; } = null!;
        public string UserName { get; set; } = null!;
        public string Password { get; set; } = null!;
        public string FullName { get; set; } = null!;
        public string IdentityCard { get; set; } = null!;
        public string Address { get; set; } = null!;
        public UserGender Gender { get; set; }
        public string? Phone { get; set; }
        public DateTime Dob { get; set; }
        public string? Avatar { get; set; }
        
    }
}

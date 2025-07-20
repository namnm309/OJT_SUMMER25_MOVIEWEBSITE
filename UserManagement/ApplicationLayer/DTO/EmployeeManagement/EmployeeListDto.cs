using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationLayer.DTO.EmployeeManagement
{
    public class EmployeeListDto
    {
        public Guid Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string IdentityCard { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
    }
}

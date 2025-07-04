using DomainLayer.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InfrastructureLayer.Core.JWT
{
    public class Payload
    {
        public Guid UserId { get; set; }
        public string Email { get; set; }
        public Guid SessionId { get; set; }
        public UserRole Role { get; set; }
        public bool IsActive { get; set; }
    }
}

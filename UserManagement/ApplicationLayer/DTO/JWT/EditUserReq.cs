using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationLayer.DTO.JWT
{
    public class EditUserReq
    {
        public string FullName { get; set; } = null!;
        public string Phone { get; set; } = null!;
        public string? Avatar { get; set; }
        public string? Address { get; set; }
    }
}

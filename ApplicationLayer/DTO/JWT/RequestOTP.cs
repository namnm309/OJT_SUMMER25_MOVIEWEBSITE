using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationLayer.DTO.JWT
{
    public class RequestOTP
    {
        public string Email { get; set; } = null!;
    }

    public class VerifyOTPChangePassword
    {
        public string email { get; set; } = null!;
        public string otp { get; set; } = null!;
        public string newPassword { get; set; } = null!;
    }
}

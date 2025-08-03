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
        
        [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "Mật khẩu mới là bắt buộc")]
        [System.ComponentModel.DataAnnotations.StringLength(100, MinimumLength = 8, ErrorMessage = "Mật khẩu phải có ít nhất 8 ký tự")]
        [System.ComponentModel.DataAnnotations.RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[!@#$%^&*()_+\-=\[\]{};':""\\|,.<>\/?~`]).{8,}$", 
            ErrorMessage = "Mật khẩu phải có ít nhất 8 ký tự, bao gồm chữ hoa, chữ thường, số và ký tự đặc biệt")]
        public string newPassword { get; set; } = null!;
    }
}

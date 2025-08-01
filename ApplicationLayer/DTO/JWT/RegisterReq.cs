using DomainLayer.Enum;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationLayer.DTO.JWT
{
    public class RegisterReq
    {
        [Required(ErrorMessage = "Email là bắt buộc")]
        [RegularExpression(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", ErrorMessage = "Email không hợp lệ")]
        public string Email { get; set; } = null!;

        [Required(ErrorMessage = "Tên đăng nhập là bắt buộc")]
        [MinLength(4, ErrorMessage = "Tên đăng nhập phải có ít nhất 4 ký tự")]
        public string UserName { get; set; } = null!;

        [Required(ErrorMessage = "Mật khẩu là bắt buộc")]
        [MinLength(6, ErrorMessage = "Mật khẩu phải có ít nhất 6 ký tự")]
        public string Password { get; set; } = null!;

        [Required(ErrorMessage = "Họ tên là bắt buộc")]
        public string FullName { get; set; } = null!;

        [Required(ErrorMessage = "CMND/CCCD là bắt buộc")]
        [RegularExpression(@"^\d{9,12}$", ErrorMessage = "CMND/CCCD phải có 9-12 chữ số")]
        public string IdentityCard { get; set; } = null!;

        [Required(ErrorMessage = "Địa chỉ là bắt buộc")]
        public string Address { get; set; } = null!;

        [Required(ErrorMessage = "Giới tính là bắt buộc")]
        public UserGender Gender { get; set; }

        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        public string? Phone { get; set; }

        [Required(ErrorMessage = "Ngày sinh là bắt buộc")]
        [DataType(DataType.Date)]
        public DateTime Dob { get; set; }

        public string? Avatar { get; set; }
        
    }
}

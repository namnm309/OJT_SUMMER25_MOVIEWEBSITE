using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace UI.Attributes
{
    public class EmailValidationAttribute : ValidationAttribute
    {
        public override bool IsValid(object? value)
        {
            if (value == null || string.IsNullOrEmpty(value.ToString()))
                return true; // Let Required attribute handle null/empty

            string email = value.ToString()!;
            
            // Kiểm tra không có khoảng trắng
            if (email.Contains(" "))
            {
                ErrorMessage = "Email không được chứa khoảng trắng";
                return false;
            }

            // Kiểm tra phải có đuôi @gmail.com
            if (!email.EndsWith("@gmail.com", StringComparison.OrdinalIgnoreCase))
            {
                ErrorMessage = "Email phải có đuôi @gmail.com";
                return false;
            }

            // Kiểm tra format email cơ bản
            var emailRegex = @"^[a-zA-Z0-9._%+-]+@gmail\.com$";
            if (!Regex.IsMatch(email, emailRegex, RegexOptions.IgnoreCase))
            {
                ErrorMessage = "Định dạng email không hợp lệ";
                return false;
            }

            return true;
        }
    }
}
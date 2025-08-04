using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace UI.Attributes
{
    public class IdentityCardValidationAttribute : ValidationAttribute
    {
        public override bool IsValid(object? value)
        {
            if (value == null || string.IsNullOrEmpty(value.ToString()))
                return true; // Let Required attribute handle null/empty

            string identityCard = value.ToString()!;
            
            // Kiểm tra độ dài 12 số (theo yêu cầu)
            if (identityCard.Length != 12)
            {
                ErrorMessage = "Số CCCD phải có đúng 12 số";
                return false;
            }

            // Kiểm tra không có khoảng trắng
            if (identityCard.Contains(" "))
            {
                ErrorMessage = "Số CCCD không được chứa khoảng trắng";
                return false;
            }

            // Kiểm tra chỉ chứa số (không có ký tự đặc biệt)
            if (!Regex.IsMatch(identityCard, @"^\d{12}$"))
            {
                ErrorMessage = "Số CCCD chỉ được chứa các chữ số, không được có ký tự đặc biệt";
                return false;
            }

            return true;
        }
    }
}
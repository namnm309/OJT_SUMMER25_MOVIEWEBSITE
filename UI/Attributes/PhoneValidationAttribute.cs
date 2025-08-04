using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace UI.Attributes
{
    public class PhoneValidationAttribute : ValidationAttribute
    {
        public override bool IsValid(object? value)
        {
            if (value == null || string.IsNullOrEmpty(value.ToString()))
                return true; // Let Required attribute handle null/empty

            string phone = value.ToString()!;
            
            // Kiểm tra độ dài 10 số và bắt đầu bằng số 0
            if (phone.Length != 10)
            {
                ErrorMessage = "Số điện thoại phải có đúng 10 số";
                return false;
            }

            if (!phone.StartsWith("0"))
            {
                ErrorMessage = "Số điện thoại phải bắt đầu bằng số 0";
                return false;
            }

            // Kiểm tra chỉ chứa số
            if (!Regex.IsMatch(phone, @"^\d{10}$"))
            {
                ErrorMessage = "Số điện thoại chỉ được chứa các chữ số";
                return false;
            }

            return true;
        }
    }
}
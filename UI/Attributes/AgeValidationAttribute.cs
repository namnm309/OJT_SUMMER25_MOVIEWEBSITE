using System.ComponentModel.DataAnnotations;

namespace UI.Attributes
{
    public class AgeValidationAttribute : ValidationAttribute
    {
        private readonly int _minimumAge;

        public AgeValidationAttribute(int minimumAge = 18)
        {
            _minimumAge = minimumAge;
        }

        public override bool IsValid(object? value)
        {
            if (value == null)
            {
                ErrorMessage = "Ngày sinh là bắt buộc";
                return false;
            }

            if (value is DateTime birthDate)
            {
                var today = DateTime.Today;
                var age = today.Year - birthDate.Year;
                
                // Kiểm tra nếu chưa đến sinh nhật trong năm nay
                if (birthDate.Date > today.AddYears(-age))
                {
                    age--;
                }

                if (age < _minimumAge)
                {
                    ErrorMessage = $"Bạn phải từ {_minimumAge} tuổi trở lên để đăng ký tài khoản";
                    return false;
                }

                return true;
            }

            ErrorMessage = "Ngày sinh không hợp lệ";
            return false;
        }
    }
}
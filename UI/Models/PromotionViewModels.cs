using System.ComponentModel.DataAnnotations;

namespace UI.Models
{
    public class PromotionViewModel
    {
        public string? Id { get; set; }

        [Required(ErrorMessage = "Vui l?ng nh?p tiêu ð?.")]
        [StringLength(100, MinimumLength = 5, ErrorMessage = "Tiêu ð? ph?i t? 5-100 k? t?")]
        public string? Title { get; set; }

        [Required(ErrorMessage = "Vui l?ng nh?p ngày b?t ð?u.")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime StartDate { get; set; }

        [Required(ErrorMessage = "Vui l?ng nh?p ngày k?t thúc.")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        [CustomValidation(typeof(PromotionViewModel), "ValidateEndDate")]
        public DateTime EndDate { get; set; }

        [Required(ErrorMessage = "Vui l?ng nh?p ph?n trãm gi?m giá")]
        [Range(1, 100, ErrorMessage = "Ph?n trãm gi?m giá ph?i t? 1 ð?n 100")]
        [Display(Name = "Ph?n trãm gi?m giá")]
        public int DiscountPercent { get; set; }

        [StringLength(500, ErrorMessage = "Mô t? không quá 500 k? t?")]
        [DataType(DataType.MultilineText)]
        public string? Description { get; set; }

        [Url(ErrorMessage = "URL h?nh ?nh không h?p l?")]
        [Display(Name = "URL h?nh ?nh")]
        public string? ImageUrl { get; set; }

        // Custom validation method
        public static ValidationResult? ValidateEndDate(DateTime endDate, ValidationContext context)
        {
            var instance = (PromotionViewModel)context.ObjectInstance;

            if (endDate < instance.StartDate)
            {
                return new ValidationResult("Ngày k?t thúc ph?i sau ngày b?t ð?u");
            }

            if (endDate > instance.StartDate.AddYears(1))
            {
                return new ValidationResult("Khuy?n m?i không ðý?c kéo dài quá 1 nãm");
            }

            return ValidationResult.Success;
        }
    }
}
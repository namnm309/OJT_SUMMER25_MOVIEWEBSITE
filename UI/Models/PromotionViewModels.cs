using System.ComponentModel.DataAnnotations;

namespace UI.Models
{
    public class PromotionViewModel
    {
        public string? Id { get; set; }

        [Required(ErrorMessage = "Vui l?ng nh?p ti�u �?.")]
        [StringLength(100, MinimumLength = 5, ErrorMessage = "Ti�u �? ph?i t? 5-100 k? t?")]
        public string? Title { get; set; }

        [Required(ErrorMessage = "Vui l?ng nh?p ng�y b?t �?u.")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime StartDate { get; set; }

        [Required(ErrorMessage = "Vui l?ng nh?p ng�y k?t th�c.")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        [CustomValidation(typeof(PromotionViewModel), "ValidateEndDate")]
        public DateTime EndDate { get; set; }

        [Required(ErrorMessage = "Vui l?ng nh?p ph?n tr�m gi?m gi�")]
        [Range(1, 100, ErrorMessage = "Ph?n tr�m gi?m gi� ph?i t? 1 �?n 100")]
        [Display(Name = "Ph?n tr�m gi?m gi�")]
        public int DiscountPercent { get; set; }

        [StringLength(500, ErrorMessage = "M� t? kh�ng qu� 500 k? t?")]
        [DataType(DataType.MultilineText)]
        public string? Description { get; set; }

        [Url(ErrorMessage = "URL h?nh ?nh kh�ng h?p l?")]
        [Display(Name = "URL h?nh ?nh")]
        public string? ImageUrl { get; set; }

        // Custom validation method
        public static ValidationResult? ValidateEndDate(DateTime endDate, ValidationContext context)
        {
            var instance = (PromotionViewModel)context.ObjectInstance;

            if (endDate < instance.StartDate)
            {
                return new ValidationResult("Ng�y k?t th�c ph?i sau ng�y b?t �?u");
            }

            if (endDate > instance.StartDate.AddYears(1))
            {
                return new ValidationResult("Khuy?n m?i kh�ng ��?c k�o d�i qu� 1 n�m");
            }

            return ValidationResult.Success;
        }
    }
}
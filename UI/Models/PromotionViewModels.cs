using System.ComponentModel.DataAnnotations;

namespace UI.Models
{
    public class PromotionViewModel
    {
        public string Id { get; set; }

        [Required(ErrorMessage = "Vui l?ng nh?p t�n khuy?n m?i")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Vui l?ng ch?n ng�y b?t �?u")]
        public DateTime StartDate { get; set; }

        [Required(ErrorMessage = "Vui l?ng ch?n ng�y k?t th�c")]
        public DateTime EndDate { get; set; }

        [Required(ErrorMessage = "Vui l?ng nh?p ph?n tr�m gi?m gi�")]
        [Range(1, 100, ErrorMessage = "Ph?n tr�m gi?m gi� ph?i t? 1 �?n 100")]
        public int DiscountPercent { get; set; }

        public string Description { get; set; }

        public string ImageUrl { get; set; }
    }
}
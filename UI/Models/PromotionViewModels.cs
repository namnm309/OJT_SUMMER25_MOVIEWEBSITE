using System.ComponentModel.DataAnnotations;

namespace UI.Models
{
    public class PromotionViewModel
    {
        public string Id { get; set; }

        [Required(ErrorMessage = "Vui l?ng nh?p tên khuy?n m?i")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Vui l?ng ch?n ngày b?t Eu")]
        public DateTime StartDate { get; set; }

        [Required(ErrorMessage = "Vui l?ng ch?n ngày k?t thúc")]
        public DateTime EndDate { get; set; }

        [Required(ErrorMessage = "Vui l?ng nh?p ph?n trãm gi?m giE")]
        [Range(1, 100, ErrorMessage = "Ph?n trãm gi?m giEph?i t? 1 En 100")]
        public int DiscountPercent { get; set; }

        public string Description { get; set; }

        public string ImageUrl { get; set; }
    }
}
using System.ComponentModel.DataAnnotations;

namespace UI.Models
{
    public class PromotionViewModel
    {
        public Guid Id { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên khuyến mãi")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn ngày bắt đầu")]
        public DateTime StartDate { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn ngày kết thúc")]
        public DateTime EndDate { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập phần trăm giảm giá")]
        [Range(1, 100, ErrorMessage = "Phần trăm giảm giá phải từ 1 đến 100")]
        public int DiscountPercent { get; set; }

        public string Description { get; set; }

        public string ImageUrl { get; set; }
    }
}

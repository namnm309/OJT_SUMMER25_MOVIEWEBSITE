using System.ComponentModel.DataAnnotations;

namespace UI.Areas.PromotionManagement.Models
{
    public class PromotionDto
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

        [Range(0, int.MaxValue, ErrorMessage = "Điểm yêu cầu phải >= 0")]
        [Display(Name = "Điểm yêu cầu (0 = miễn phí)")]
        public double RequiredPoints { get; set; } = 0;

        public string Description { get; set; }

        public string ImageUrl { get; set; }
    }
}

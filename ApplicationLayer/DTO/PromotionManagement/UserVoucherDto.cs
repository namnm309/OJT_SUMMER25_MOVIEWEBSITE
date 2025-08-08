using System;

namespace ApplicationLayer.DTO.PromotionManagement
{
    public class UserVoucherDto
    {
        public Guid UserPromotionId { get; set; }
        public Guid PromotionId { get; set; }
        public string Title { get; set; } = string.Empty;
        public int DiscountPercent { get; set; }
        public double RequiredPoints { get; set; }
        public string Description { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string? ImageUrl { get; set; }
        public bool IsRedeemed { get; set; }
        public DateTime? RedeemedAt { get; set; }
    }
} 
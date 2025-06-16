using System.ComponentModel.DataAnnotations;

namespace UI.Models
{
    public class PromotionCreateViewModel
    {
        [Required]
        [MaxLength(100)]
        public string Title { get; set; } = string.Empty;
        
        [Required]
        public DateTime StartDate { get; set; }
        
        [Required]
        public DateTime EndDate { get; set; }
        
        [Required]
        [Range(0, 999999.99)]
        public decimal PromotionValue { get; set; }
        
        [Required]
        public string PromotionType { get; set; } = "Percentage"; // Percentage or FixedAmount
        
        [Required]
        [MaxLength(2000)]
        public string Description { get; set; } = string.Empty;
        
        [MaxLength(500)]
        public string? ImageUrl { get; set; }
    }

    public class PromotionUpdateViewModel : PromotionCreateViewModel
    {
        // Inherits all fields from Create
    }
} 
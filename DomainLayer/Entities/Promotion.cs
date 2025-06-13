using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DomainLayer.Enum;

namespace DomainLayer.Entities
{
    [Table("tbl_promotions")]
    public class Promotion
    {
        [Key]
        public Guid Id { get; set; }
        
        [Required]
        [StringLength(100)]
        public string Title { get; set; }
        
        [Required]
        public DateTime StartDate { get; set; }
        
        [Required]
        public DateTime EndDate { get; set; }
        
        // Giá trị khuyến mãi (có thể là % hoặc số tiền cố định)
        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal PromotionValue { get; set; }
        
        // Loại khuyến mãi: Percentage hoặc FixedAmount
        [Required]
        public PromotionType PromotionType { get; set; } = PromotionType.Percentage;
        
        [Required]
        public string Description { get; set; }
        
        public string ImageUrl { get; set; }
        
        public DateTime CreatedAt { get; set; }
        
        public DateTime UpdatedAt { get; set; }
    }
} 
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DomainLayer.Entities
{
    [Table("tbl_user_promotions")]
    public class UserPromotion : BaseEntity
    {
        [Required]
        public Guid UserId { get; set; }

        [Required]
        public Guid PromotionId { get; set; }

        public bool IsRedeemed { get; set; } = false;

        public DateTime? RedeemedAt { get; set; }

        // Navigation properties
        [ForeignKey("UserId")]
        public virtual Users User { get; set; } = null!;

        [ForeignKey("PromotionId")]
        public virtual Promotion Promotion { get; set; } = null!;
    }
} 
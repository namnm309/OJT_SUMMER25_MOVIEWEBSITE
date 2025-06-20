using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DomainLayer.Enum;

namespace DomainLayer.Entities
{
    [Table("tbl_point_histories")]
    public class PointHistory : BaseEntity
    {
        [Required]
        public double Points { get; set; }

        [Required]
        public PointType Type { get; set; }

        [MaxLength(255)]
        public string? Description { get; set; }

        [Required]
        public bool IsUsed { get; set; } // true = use point, false = plus point

        // Foreign keys
        [Required]
        public Guid UserId { get; set; }

        public Guid? BookingId { get; set; }

        // Navigation properties
        [ForeignKey("UserId")]
        public virtual Users User { get; set; } = null!;

        [ForeignKey("BookingId")]
        public virtual Booking? Booking { get; set; }
    }
} 
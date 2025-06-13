using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DomainLayer.Entities
{
    [Table("tbl_booking_details")]
    public class BookingDetail
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid BookingDetailId { get; set; }

        [Required]
        public decimal Price { get; set; }

        // Foreign keys
        [Required]
        public Guid BookingId { get; set; }

        [Required]
        public Guid SeatId { get; set; }

        // Navigation properties
        [ForeignKey("BookingId")]
        public virtual Booking Booking { get; set; } = null!;

        [ForeignKey("SeatId")]
        public virtual Seat Seat { get; set; } = null!;
    }
} 
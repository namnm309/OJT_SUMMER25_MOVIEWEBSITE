using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DomainLayer.Enum;

namespace DomainLayer.Entities
{
    [Table("tbl_bookings")]
    public class Booking : BaseEntity
    {
        [Required]
        [MaxLength(100)]
        public string FullName { get; set; }

        [Required]
        [MaxLength(100)]
        public string Email { get; set; }

        [Required]
        [MaxLength(20)]
        public string PhoneNumber { get; set; }

        [Required]
        [MaxLength(20)]
        public string IdentityCardNumber { get; set; }

        [Required]
        [MaxLength(20)]
        public string BookingCode { get; set; } = string.Empty;

        [Required]
        public DateTime BookingDate { get; set; } = DateTime.UtcNow;

        [Required]
        public decimal TotalPrice { get; set; }

        [Required]
        public BookingStatus Status { get; set; } = BookingStatus.Pending;

        [Required]
        public int TotalSeats { get; set; }

        // Số vé được chuyển đổi từ điểm thành viên
        public int? ConvertedTickets { get; set; }

        // Điểm được sử dụng để chuyển đổi vé
        public double? PointsUsed { get; set; }

        // Foreign keys
        [Required]
        public Guid UserId { get; set; }

        [Required]
        public Guid ShowTimeId { get; set; }

        // Navigation properties
        [ForeignKey("UserId")]
        public virtual Users User { get; set; } = null!;

        [ForeignKey("ShowTimeId")]
        public virtual ShowTime ShowTime { get; set; } = null!;

        public virtual ICollection<BookingDetail> BookingDetails { get; set; } = new List<BookingDetail>();
        
        public virtual ICollection<PointHistory> PointHistories { get; set; } = new List<PointHistory>();
    }
} 
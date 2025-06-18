using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DomainLayer.Enum;

namespace DomainLayer.Entities
{
    [Table("tbl_seats")]
    public class Seat : BaseEntity
    {
        [Required]
        [MaxLength(10)]
        public string SeatCode { get; set; } = string.Empty;

        [Required]
        public SeatType SeatType { get; set; } = SeatType.Normal;

        [Required]
        public int RowIndex { get; set; }

        [Required]
        public int ColumnIndex { get; set; }

        [Required]
        public bool IsActive { get; set; } = true;

        // Foreign key
        [Required]
        public Guid RoomId { get; set; }

        // Navigation property
        [ForeignKey("RoomId")]
        public virtual CinemaRoom Room { get; set; } = null!;
        
        public virtual ICollection<BookingDetail> BookingDetails { get; set; } = new List<BookingDetail>();
    }
} 
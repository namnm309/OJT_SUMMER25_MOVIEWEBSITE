using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainLayer.Entities
{
    [Table("tbl_tickets")]
    public class Ticket : BaseEntity
    {
        [Required]
        [StringLength(100)]
        public string MovieName { get; set; } = string.Empty;

        [Required]
        public string Screen { get; set; } = string.Empty;

        [Required]
        public DateTime ShowDate { get; set; }

        [Required]
        public TimeSpan ShowTime { get; set; }

        [Required]
        public string SeatCode { get; set; } = string.Empty;

        [Required]
        public decimal Price { get; set; }

        // Foreign key
        [Required]
        public Guid BookingId { get; set; }

        [Required]
        public Guid ShowTimeId { get; set; }

        // Navigation
        [ForeignKey("BookingId")]
        public virtual Booking Booking { get; set; } = null!;

        [ForeignKey("ShowTimeId")]
        public virtual ShowTime ShowTimeRef { get; set; } = null!;
    }
}

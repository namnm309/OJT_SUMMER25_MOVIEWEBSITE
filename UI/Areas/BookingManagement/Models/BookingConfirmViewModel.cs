using System.ComponentModel.DataAnnotations;

namespace UI.Areas.BookingManagement.Models
{
    public class BookingConfirmViewModel
    {
        [Required]
        public Guid UserId { get; set; }

        [Required]
        public Guid ShowTimeId { get; set; }

        [Required]
        public List<Guid> SeatIds { get; set; } = new List<Guid>();

        [Required]
        public decimal TotalPrice { get; set; }

        [Required]
        public int TotalSeats { get; set; }

        public int? ConvertedTickets { get; set; }

        public double? PointsUsed { get; set; }

        public string? Notes { get; set; }
    }
} 
using System.ComponentModel.DataAnnotations;

namespace UI.Areas.BookingManagement.Models
{
    public class TicketSaleConfirmViewModel
    {
        [Required]
        public Guid ShowTimeId { get; set; }

        [Required]
        public List<Guid> SeatIds { get; set; } = new List<Guid>();

        [Required]
        public decimal TotalPrice { get; set; }

        [Required]
        public int TotalSeats { get; set; }

        public string? MemberId { get; set; }

        public string? MemberIdentityCard { get; set; }

        public bool UseScoreConversion { get; set; }

        public int? ConvertedTickets { get; set; }

        public double? PointsUsed { get; set; }

        public string CustomerName { get; set; } = string.Empty;

        public string CustomerPhone { get; set; } = string.Empty;

        public string CustomerIdentityCard { get; set; } = string.Empty;
    }
} 
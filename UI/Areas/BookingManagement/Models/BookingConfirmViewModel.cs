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

    /// <summary>
    /// ViewModel for admin booking confirmation
    /// </summary>
    public class ConfirmAdminBookingViewModel
    {
        [Required]
        public Guid ShowTimeId { get; set; }

        [Required]
        public List<Guid> SeatIds { get; set; } = new List<Guid>();

        public string? MemberId { get; set; }

        public int? ConvertedTickets { get; set; }

        public double? PointsUsed { get; set; }

        [Required]
        public string PaymentMethod { get; set; } = "cash";

        [Required]
        public string StaffId { get; set; }
    }

    /// <summary>
    /// ViewModel for booking confirmation response
    /// </summary>
    public class BookingConfirmationViewModel
    {
        public string BookingCode { get; set; }
        public string MovieTitle { get; set; }
        public string CinemaRoom { get; set; }
        public string ShowDate { get; set; }
        public string ShowTime { get; set; }
        public List<SeatViewModel> Seats { get; set; } = new List<SeatViewModel>();
        public decimal SubTotal { get; set; }
        public decimal Discount { get; set; }
        public decimal Total { get; set; }
        public double PointsUsed { get; set; }
        public double RemainingPoints { get; set; }
        public string PaymentMethod { get; set; }
        public string BookingDate { get; set; }
    }

    public class SeatViewModel
    {
        public Guid Id { get; set; }
        public string SeatCode { get; set; }
        public decimal Price { get; set; }
    }
} 
using System.ComponentModel.DataAnnotations;

namespace UI.Areas.BookingManagement.Models
{
    /// <summary>
    /// ViewModel for booking confirmation detail display (AC-01)
    /// </summary>
    public class BookingConfirmationDetailViewModel
    {
        // Booking Information (Read-only fields)
        public string BookingId { get; set; }
        public string MovieName { get; set; }
        public string Screen { get; set; }
        public string Date { get; set; }
        public string Time { get; set; }
        public string Seat { get; set; }
        public decimal Price { get; set; }
        public decimal Total { get; set; }

        // Member Information (Read-only fields)
        public string MemberId { get; set; }
        public string FullName { get; set; }
        public double MemberScore { get; set; }
        public string IdentityCard { get; set; }
        public string PhoneNumber { get; set; }

        // Additional Info for score conversion
        public List<SeatDetailViewModel> SeatDetails { get; set; } = new List<SeatDetailViewModel>();
        public bool CanConvertScore { get; set; }
        public int MaxTicketsFromScore { get; set; }
        public double ScorePerTicket { get; set; }
    }

    /// <summary>
    /// ViewModel for seat details
    /// </summary>
    public class SeatDetailViewModel
    {
        public Guid Id { get; set; }
        public string SeatCode { get; set; }
        public decimal Price { get; set; }
    }

    /// <summary>
    /// Request ViewModel for booking confirmation with score conversion (AC-02, AC-05)
    /// </summary>
    public class BookingConfirmWithScoreViewModel
    {
        [Required]
        public Guid ShowTimeId { get; set; }

        [Required]
        public List<Guid> SeatIds { get; set; } = new List<Guid>();

        [Required]
        public string MemberId { get; set; }
        
        // Score conversion options (AC-02)
        public bool UseScoreConversion { get; set; }
        
        [Range(0, int.MaxValue, ErrorMessage = "Số vé chuyển đổi phải lớn hơn hoặc bằng 0")]
        public int TicketsToConvert { get; set; } = 0;
        
        // Payment and staff info
        [Required]
        public string PaymentMethod { get; set; } = "cash";
        
        [Required]
        public string StaffId { get; set; }
        
        public string Notes { get; set; }
    }

    /// <summary>
    /// Response ViewModel after successful booking confirmation
    /// </summary>
    public class BookingConfirmSuccessViewModel
    {
        public string BookingCode { get; set; }
        public string MovieTitle { get; set; }
        public string CinemaRoom { get; set; }
        public string ShowDate { get; set; }
        public string ShowTime { get; set; }
        public List<SeatDetailViewModel> Seats { get; set; } = new List<SeatDetailViewModel>();
        
        // Financial summary
        public decimal SubTotal { get; set; }
        public decimal ScoreDiscount { get; set; }
        public decimal Total { get; set; }
        
        // Score usage details
        public bool ScoreUsed { get; set; }
        public int TicketsConvertedFromScore { get; set; }
        public double ScoreDeducted { get; set; }
        public double RemainingScore { get; set; }
        
        // Other info
        public string PaymentMethod { get; set; }
        public string BookingDate { get; set; }
        public string Message { get; set; }
    }
} 
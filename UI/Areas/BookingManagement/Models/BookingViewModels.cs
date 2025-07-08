using System.ComponentModel.DataAnnotations;

namespace UI.Areas.BookingManagement.Models
{
    // T8: Select Movie and Showtime ViewModels
    public class SelectMovieViewModel
    {
        public List<MovieOption> Movies { get; set; } = new List<MovieOption>();
        public Guid? SelectedMovieId { get; set; }
        public DateTime? SelectedDate { get; set; }
        public Guid? SelectedShowTimeId { get; set; }
        public List<DateTime> AvailableDates { get; set; } = new List<DateTime>();
        public List<ShowTimeOption> AvailableShowTimes { get; set; } = new List<ShowTimeOption>();
    }

    public class MovieOption
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Poster { get; set; } = string.Empty;
        public int Duration { get; set; }
        public string Genre { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public bool IsAvailable { get; set; } = true;
    }

    public class ShowTimeOption
    {
        public Guid Id { get; set; }
        public Guid MovieId { get; set; }
        public Guid CinemaRoomId { get; set; }
        public string CinemaRoomName { get; set; } = string.Empty;
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public decimal Price { get; set; }
        public int AvailableSeats { get; set; }
        public int TotalSeats { get; set; }
    }

    // T9: Select Seats ViewModels
    public class SelectSeatViewModel
    {
        public Guid ShowTimeId { get; set; }
        public string MovieTitle { get; set; } = string.Empty;
        public string CinemaRoom { get; set; } = string.Empty;
        public DateTime ShowTime { get; set; }
        public int MaxSeats { get; set; } = 8;
        public int SelectedSeatCount { get; set; } = 0;
        public List<Guid> SelectedSeatIds { get; set; } = new List<Guid>();
        public List<SeatInfo> Seats { get; set; } = new List<SeatInfo>();
        public decimal TotalPrice { get; set; }
        public decimal RegularSeatPrice { get; set; }
        public decimal VipSeatPrice { get; set; }
    }

    public class SeatInfo
    {
        public Guid Id { get; set; }
        public string SeatNumber { get; set; } = string.Empty;
        public int Row { get; set; }
        public int Column { get; set; }
        public SeatType Type { get; set; }
        public bool IsOccupied { get; set; }
        public bool IsSelected { get; set; }
        public decimal Price { get; set; }
    }

    public enum SeatType
    {
        Regular = 1,
        VIP = 2
    }

    // T10: Confirm Booking ViewModels  
    public class ConfirmBookingViewModel
    {
        public Guid ShowTimeId { get; set; }
        public string MovieTitle { get; set; } = string.Empty;
        public string MoviePoster { get; set; } = string.Empty;
        public string CinemaRoom { get; set; } = string.Empty;
        public DateTime ShowDate { get; set; }
        public TimeSpan ShowTime { get; set; }
        public List<string> SelectedSeats { get; set; } = new List<string>();
        public List<Guid> SelectedSeatIds { get; set; } = new List<Guid>();
        public decimal PricePerTicket { get; set; }
        public decimal TotalPrice { get; set; }
        public int TotalSeats { get; set; }

        // Customer Info (từ User hiện tại)
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerPhone { get; set; } = string.Empty;
        public string CustomerEmail { get; set; } = string.Empty;
        public string CustomerIdCard { get; set; } = string.Empty;

        // Points/Promotion
        public int AvailablePoints { get; set; }
        public bool UsePoints { get; set; } = false;
        public int PointsToUse { get; set; } = 0;
        public decimal DiscountFromPoints { get; set; } = 0;
        public decimal FinalPrice { get; set; }

        [Display(Name = "Ghi chú")]
        public string? Notes { get; set; }
    }

    // T11: Ticket Information ViewModels
    public class TicketInfoViewModel
    {
        public Guid BookingId { get; set; }
        public string BookingCode { get; set; } = string.Empty;
        public string MovieTitle { get; set; } = string.Empty;
        public string MoviePoster { get; set; } = string.Empty;
        public string CinemaRoom { get; set; } = string.Empty;
        public DateTime ShowDate { get; set; }
        public TimeSpan ShowTime { get; set; }
        public List<string> SeatNumbers { get; set; } = new List<string>();
        public decimal TotalPrice { get; set; }
        public DateTime BookingTime { get; set; }
        public BookingStatus Status { get; set; }

        // Customer Info
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerPhone { get; set; } = string.Empty;
        public string CustomerEmail { get; set; } = string.Empty;

        // Points Used
        public int PointsUsed { get; set; }
        public decimal PointsDiscount { get; set; }
        public int PointsEarned { get; set; }

        public string? Notes { get; set; }
    }

    public enum BookingStatus
    {
        Pending = 1,
        Confirmed = 2,
        Cancelled = 3,
        Completed = 4
    }

    // API Response Models
    public class BookingResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public Guid? BookingId { get; set; }
        public string? BookingCode { get; set; }
    }

    // Search Movies
    public class SearchMovieViewModel
    {
        public string SearchTerm { get; set; } = string.Empty;
        public List<MovieOption> Results { get; set; } = new List<MovieOption>();
        public int TotalResults { get; set; }
        public bool HasSearched { get; set; } = false;
    }

    public class ProcessBookingRequest
    {
        public string ShowtimeId { get; set; }
        public List<string> SeatIds { get; set; } = new List<string>();
        public decimal TotalPrice { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string IdentityCard { get; set; }
        public string PhoneNumber { get; set; }
    }

    public class BookingResultViewModel
    {
        public bool Success { get; set; }
        public dynamic Data { get; set; }
        public ProcessBookingRequest BookingInfo { get; set; }
    }
}
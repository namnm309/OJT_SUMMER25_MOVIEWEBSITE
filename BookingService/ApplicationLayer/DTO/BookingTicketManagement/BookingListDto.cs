using System.ComponentModel.DataAnnotations;

namespace ApplicationLayer.DTO.BookingTicketManagement
{
    public class BookingListDto
    {
        public Guid Id { get; set; }
        public string BookingCode { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerPhone { get; set; } = string.Empty;
        public string CustomerEmail { get; set; } = string.Empty;
        public string MovieTitle { get; set; } = string.Empty;
        public string CinemaRoom { get; set; } = string.Empty;
        public DateTime ShowDate { get; set; }
        public TimeSpan ShowTime { get; set; }
        public string SeatNumbers { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public string BookingStatus { get; set; } = string.Empty;
        public DateTime BookingDate { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public int UsedPoints { get; set; }
    }

    public class BookingFilterDto
    {
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string? MovieTitle { get; set; }
        public string? BookingStatus { get; set; }
        public string? CustomerSearch { get; set; } // Search by name, phone, email
        public string? BookingCode { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string SortBy { get; set; } = "BookingDate";
        public string SortDirection { get; set; } = "desc";
    }

    public class BookingListResponseDto
    {
        public List<BookingListDto> Bookings { get; set; } = new List<BookingListDto>();
        public int TotalRecords { get; set; }
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public int PageSize { get; set; }
    }

    public class UpdateBookingStatusDto
    {
        [Required]
        public string NewStatus { get; set; } = string.Empty;
    }

    public class CancelBookingDto
    {
        [Required]
        public string Reason { get; set; } = string.Empty;
    }
} 
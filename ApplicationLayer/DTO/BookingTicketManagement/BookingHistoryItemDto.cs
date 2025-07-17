namespace ApplicationLayer.DTO.BookingTicketManagement
{
    public class BookingHistoryItemDto
    {
        public Guid BookingId { get; set; }
        public string BookingCode { get; set; } = string.Empty;
        public string MovieTitle { get; set; } = string.Empty;
        public string CinemaRoom { get; set; } = string.Empty;
        public DateTime ShowDate { get; set; }
        public TimeSpan ShowTime { get; set; }
        public decimal Amount { get; set; }
        public string Status { get; set; } = string.Empty; // e.g., Confirmed / Canceled
        public DateTime BookingDate { get; set; }
        public bool IsRefund { get; set; }
    }
} 
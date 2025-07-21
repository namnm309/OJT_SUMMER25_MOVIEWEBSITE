namespace ApplicationLayer.DTO.BookingTicketManagement
{
    public class ConfirmBookingRequestDto
    {
        public Guid ShowtimeId { get; set; }
        public List<Guid> SeatIds { get; set; } = new List<Guid>();
        public decimal TotalPrice { get; set; }
        public Guid UserId { get; set; } // Giả định có ID người dùng từ token hoặc context
        public string FullName { get; set; } // Thông tin người dùng
        public string Email { get; set; }
        public string IdentityCard { get; set; }
        public string PhoneNumber { get; set; }
        // Các trường khác nếu cần thiết cho thông tin đặt vé chi tiết
        // VD: MovieTitle, CinemaRoomName, ShowDate, ShowTime (cho mục đích hiển thị/ghi log, không cần thiết để lưu vào DB nếu có thể join từ ShowtimeId)
    }
}
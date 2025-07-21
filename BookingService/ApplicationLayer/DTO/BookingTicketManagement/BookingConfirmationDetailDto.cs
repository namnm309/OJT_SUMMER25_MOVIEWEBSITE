using System;
using System.Collections.Generic;

namespace ApplicationLayer.DTO.BookingTicketManagement
{
    /// <summary>
    /// DTO for displaying detailed booking information (AC-01)
    /// </summary>
    public class BookingConfirmationDetailDto
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

        // Additional Info
        public List<SeatDetailForConfirmDto> SeatDetails { get; set; } = new List<SeatDetailForConfirmDto>();
        public bool CanConvertScore { get; set; }
        public int MaxTicketsFromScore { get; set; }
        public double ScorePerTicket { get; set; }
    }

    /// <summary>
    /// DTO for seat details in confirmation
    /// </summary>
    public class SeatDetailForConfirmDto
    {
        public Guid Id { get; set; }
        public string SeatCode { get; set; }
        public decimal Price { get; set; }
    }

    /// <summary>
    /// Request DTO for booking confirmation with score conversion (AC-02, AC-05)
    /// </summary>
    public class BookingConfirmWithScoreRequestDto
    {
        public Guid ShowTimeId { get; set; }
        public List<Guid> SeatIds { get; set; } = new List<Guid>();
        public string MemberId { get; set; }
        
        // Score conversion options (AC-02)
        public bool UseScoreConversion { get; set; }
        public int TicketsToConvert { get; set; } = 0;
        
        // Payment and staff info
        public string PaymentMethod { get; set; } = "cash";
        public string StaffId { get; set; }
        public string Notes { get; set; }
        // Thêm promotionId để áp dụng khuyến mãi
        public Guid? PromotionId { get; set; }
    }

    /// <summary>
    /// Response DTO after successful booking confirmation
    /// </summary>
    public class BookingConfirmSuccessDto
    {
        public string BookingCode { get; set; }
        public string MovieTitle { get; set; }
        public string CinemaRoom { get; set; }
        public string ShowDate { get; set; }
        public string ShowTime { get; set; }
        public List<SeatDetailForConfirmDto> Seats { get; set; } = new List<SeatDetailForConfirmDto>();
        
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
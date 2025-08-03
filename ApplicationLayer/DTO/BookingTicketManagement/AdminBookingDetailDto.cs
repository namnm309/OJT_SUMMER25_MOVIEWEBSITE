using System;
using System.Collections.Generic;

namespace ApplicationLayer.DTO.BookingTicketManagement
{
    public class AdminBookingDetailDto
    {
        public Guid Id { get; set; }
        public string BookingCode { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string MovieTitle { get; set; } = string.Empty;
        public string CinemaRoom { get; set; } = string.Empty;
        public DateTime? ShowDate { get; set; }
        public string ShowTime { get; set; } = string.Empty;
        public List<string> SeatCodes { get; set; } = new List<string>();
        public decimal TotalPrice { get; set; }
        public DateTime BookingDate { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        
        // User information
        public Guid? UserId { get; set; }
        public string? UserFullName { get; set; }
        public string? UserEmail { get; set; }
        public string? UserPhone { get; set; }
        public string? UserIdentityCard { get; set; }
    }
} 
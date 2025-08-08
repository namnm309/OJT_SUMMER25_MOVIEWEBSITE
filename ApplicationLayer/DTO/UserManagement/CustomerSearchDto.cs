using System;

namespace ApplicationLayer.DTO.UserManagement
{
    public class CustomerSearchDto
    {
        public Guid Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public int Points { get; set; } = 0;
        public int TotalBookings { get; set; } = 0;
        public DateTime? LastBookingDate { get; set; }
    }
} 
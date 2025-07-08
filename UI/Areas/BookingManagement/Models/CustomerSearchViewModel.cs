using System.ComponentModel.DataAnnotations;

namespace UI.Areas.BookingManagement.Models
{
    public class CustomerSearchViewModel
    {
        public string Id { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public int Points { get; set; }
        public int TotalBookings { get; set; }
        public DateTime? LastBookingDate { get; set; }
    }

    public class CreateCustomerViewModel
    {
        public string FullName { get; set; }
        public string IdentityCard { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
    }
} 
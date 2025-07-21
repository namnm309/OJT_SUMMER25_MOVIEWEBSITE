using Microsoft.EntityFrameworkCore.Diagnostics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationLayer.DTO.BookingTicketManagement
{
    public class BookingDetailsDto
    {
        public string MovieName { get; set; } = string.Empty;

        public Guid BookingId { get; set; }
        public string BookingCode { get; set; } = string.Empty;

        public string RoomName { get; set; } = string.Empty;

        public DateTime ShowDate { get; set; }

        public List<string> SeatCodes { get; set; } = new List<string>();

        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
        public int TotalSeats { get; set; }

        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string IdentityCard { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
    }
}

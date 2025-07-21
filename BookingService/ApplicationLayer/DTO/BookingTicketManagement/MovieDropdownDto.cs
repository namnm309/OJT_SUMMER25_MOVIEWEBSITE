using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationLayer.DTO.BookingTicketManagement
{
    public class MovieDropdownDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? PrimaryImageUrl { get; set; }
        public string? Genre { get; set; }
        public int? Duration { get; set; }
    }
}

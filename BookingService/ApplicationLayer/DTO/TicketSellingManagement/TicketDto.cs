using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationLayer.DTO.TicketSellingManagement
{
    public class TicketDto
    {
        public Guid Id { get; set; }
        public string MovieName { get; set; } = string.Empty;
        public string Screen { get; set; } = string.Empty;
        public DateTime ShowDate { get; set; }
        public TimeSpan ShowTime { get; set; }
        public string SeatCode { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public Guid BookingId { get; set; }
        public Guid ShowTimeId { get; set; }
    }
}

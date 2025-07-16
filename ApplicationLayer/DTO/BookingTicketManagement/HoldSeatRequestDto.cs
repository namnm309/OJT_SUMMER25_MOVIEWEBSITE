using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationLayer.DTO.BookingTicketManagement
{
    public class HoldSeatRequestDto
    {
        public Guid ShowTimeId { get; set; }
        public List<Guid> SeatIds { get; set; } = new();
    }
}

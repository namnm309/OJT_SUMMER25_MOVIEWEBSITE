using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationLayer.DTO.BookingTicketManagement
{
    public class HoldSeatSignalRequest
    {
        public Guid ShowTimeId { get; set; }
        public List<Guid> SeatIds { get; set; }
    }

    public class ReleaseSeatSignalRequest
    {
        public Guid ShowTimeId { get; set; }
        public List<Guid> SeatIds { get; set; }
    }

    public class HoldSeatResult
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public Guid? BookingId { get; set; }
        public string? BookingCode { get; set; }
        public DateTime? ExpiredAt { get; set; }
    }
}

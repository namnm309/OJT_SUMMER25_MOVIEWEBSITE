using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationLayer.DTO.BookingTicketManagement
{
    public class HoldSeatRequestDto
    {
        public List<Guid> SeatIds { get; set; } = new();
        public Guid ShowTimeId { get; set; }
    }

    public class SeatSummaryRequestDto
    {
        [Required]
        public Guid SeatLogId { get; set; }

        public Guid? PromotionId { get; set; }
    }

    public class SeatSummaryDto
    {
        public Guid BookingId { get; set; }
        public string BookingCode { get; set; } = string.Empty;
        public List<string> SeatCodes { get; set; } = new();
        public int Quantity { get; set; }
        public decimal TotalPrice { get; set; }
        public int DiscountPercent { get; set; } = 0;
        public decimal FinalPrice { get; set; }
        public DateTime ExpiredAt { get; set; }

    }

    public class ReleaseSeatRequestDto
    {
        public Guid SeatLogId { get; set; }
    }
}

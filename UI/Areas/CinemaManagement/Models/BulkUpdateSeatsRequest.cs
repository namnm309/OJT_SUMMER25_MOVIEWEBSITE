using System.ComponentModel.DataAnnotations;

namespace UI.Areas.CinemaManagement.Models
{
    public class BulkUpdateSeatsRequest
    {
        [Required]
        public Guid RoomId { get; set; }
        
        [Required]
        public List<BulkSeatUpdate> Updates { get; set; } = new List<BulkSeatUpdate>();
    }

    public class BulkSeatUpdate
    {
        [Required]
        public Guid SeatId { get; set; }
        
        public int? NewSeatType { get; set; } // 0=Normal, 1=VIP, 2=Couple
        
        public decimal? NewPrice { get; set; }
    }
} 
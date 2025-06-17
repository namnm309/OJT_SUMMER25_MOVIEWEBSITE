using System.ComponentModel.DataAnnotations;

namespace UI.Models
{
    public class CinemaRoomCreateViewModel
    {
        [Required]
        [MaxLength(50)]
        public string RoomName { get; set; } = string.Empty;
        
        [Required]
        [Range(1, 500)]
        public int TotalSeats { get; set; }
    }

    public class SeatUpdateViewModel
    {
        public Guid SeatId { get; set; }
        
        [Required]
        [MaxLength(10)]
        public string SeatCode { get; set; } = string.Empty;
        
        [Required]
        public string SeatType { get; set; } = "Normal"; // Normal, VIP
        
        [Required]
        public int RowIndex { get; set; }
        
        [Required]
        public int ColumnIndex { get; set; }
        
        public bool IsActive { get; set; } = true;
    }
} 
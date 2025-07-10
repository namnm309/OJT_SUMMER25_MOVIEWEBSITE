using System.ComponentModel.DataAnnotations;

namespace UI.Areas.CinemaManagement.Models
{
    public class CinemaRoomCreateViewModel
    {
        [Required]
        [MaxLength(50)]
        public string RoomName { get; set; } = string.Empty;
        
        [Required]
        [Range(1, 500)]
        public int TotalSeats { get; set; }

        [Required]
        [Range(1, 50, ErrorMessage = "Số hàng phải từ 1 đến 50")]
        public int NumberOfRows { get; set; }

        [Required]
        [Range(1, 50, ErrorMessage = "Số cột phải từ 1 đến 50")]
        public int NumberOfColumns { get; set; }
    }

    public class CinemaRoomUpdateViewModel
    {
        [MaxLength(50)]
        public string? RoomName { get; set; }
        
        [Range(1, 500)]
        public int? TotalSeats { get; set; }
        
        public bool? IsActive { get; set; }

        [Range(1, 50, ErrorMessage = "Số hàng phải từ 1 đến 50")]
        public int? NumberOfRows { get; set; }

        [Range(1, 50, ErrorMessage = "Số cột phải từ 1 đến 50")]
        public int? NumberOfColumns { get; set; }
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
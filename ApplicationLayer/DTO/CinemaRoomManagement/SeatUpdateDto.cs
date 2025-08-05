using DomainLayer.Enum;
using System.ComponentModel.DataAnnotations;

namespace ApplicationLayer.DTO.CinemaRoomManagement
{
    public class SeatUpdateDto
    {
        public Guid SeatId { get; set; }
        
        [Required]
        [MaxLength(10)]
        public string SeatCode { get; set; } = string.Empty;
        
        [Required]
        public SeatType SeatType { get; set; }
        
        [Required]
        public int RowIndex { get; set; }
        
        [Required]
        public int ColumnIndex { get; set; }
        
        [Required]
        [Range(0, 1000000, ErrorMessage = "Giá ghế phải từ 0 đến 1,000,000 VNĐ")]
        public decimal PriceSeat { get; set; }
        
        public bool IsActive { get; set; } = true;
    }
} 
using System.ComponentModel.DataAnnotations;

namespace ApplicationLayer.DTO.CinemaRoomManagement
{
    public class CinemaRoomUpdateDto
    {
        [MaxLength(50)]
        public string? RoomName { get; set; }

        [Range(1, 1000)]
        public int? TotalSeats { get; set; }

        public bool? IsActive { get; set; }

        [Range(1, 50, ErrorMessage = "Số hàng phải từ 1 đến 50")]
        public int? NumberOfRows { get; set; }

        [Range(1, 50, ErrorMessage = "Số cột phải từ 1 đến 50")]  
        public int? NumberOfColumns { get; set; }

        public decimal? DefaultSeatPrice { get; set; }

        // Flag để xác nhận việc tái tạo ghế (sẽ xóa tất cả ghế cũ)
        public bool RegenerateSeats { get; set; } = false;
    }
} 
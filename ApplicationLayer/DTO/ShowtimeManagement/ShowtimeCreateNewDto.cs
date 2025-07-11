using System;
using System.ComponentModel.DataAnnotations;

namespace ApplicationLayer.DTO.ShowtimeManagement
{
    public class ShowtimeCreateNewDto
    {
        [Required(ErrorMessage = "Vui lòng chọn phim")]
        public Guid MovieId { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn phòng chiếu")]
        public Guid CinemaRoomId { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn ngày chiếu")]
        public DateTime ShowDate { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập giờ bắt đầu")]
        public TimeSpan StartTime { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập giờ kết thúc")]
        public TimeSpan EndTime { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập giá vé")]
        [Range(0, 1000000, ErrorMessage = "Giá vé phải từ 0 đến 1,000,000 VNĐ")]
        public decimal Price { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
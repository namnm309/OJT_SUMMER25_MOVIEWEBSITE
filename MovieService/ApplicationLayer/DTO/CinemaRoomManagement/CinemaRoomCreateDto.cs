using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationLayer.DTO.CinemaRoomManagement
{
    public class CinemaRoomCreateDto
    {
        [Required]
        [MaxLength(50)]
        public string RoomName { get; set; } = null!;

        [Range(1, 1000)]
        public int TotalSeats { get; set; }

        [Range(1, 50, ErrorMessage = "Số hàng phải từ 1 đến 50")]
        public int NumberOfRows { get; set; } = 10;

        [Range(1, 50, ErrorMessage = "Số cột phải từ 1 đến 50")]  
        public int NumberOfColumns { get; set; } = 10;

        public decimal DefaultSeatPrice { get; set; } = 100000; // Giá ghế mặc định
    }
}

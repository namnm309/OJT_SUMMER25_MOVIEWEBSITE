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

    }
}

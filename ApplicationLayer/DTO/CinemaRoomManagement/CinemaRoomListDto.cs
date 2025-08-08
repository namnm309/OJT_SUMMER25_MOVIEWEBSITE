using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationLayer.DTO.CinemaRoomManagement
{
    public class CinemaRoomListDto
    {
        public Guid Id { get; set; }
        public string RoomName { get; set; }
        public int TotalSeats { get; set; }
    }
}

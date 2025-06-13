using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationLayer.DTO.CinemaRoomManagement
{
    public class CinemaRoomDto
    {
        /// The unique identifier of the cinema room.
        public Guid RoomId { get; set; }

        /// The display name of the cinema room.
        public string RoomName { get; set; } = string.Empty;

        /// Total number of seats in this room.
        public int TotalSeats { get; set; }


    }
}

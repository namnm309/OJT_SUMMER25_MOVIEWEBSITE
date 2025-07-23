using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationLayer.DTO.CinemaRoomManagement
{
    public class ShowTimeDetailsDto
    {
        public Guid MovieId { get; set; }
        public Guid RoomId { get; set; }
        public DateTime? ShowDate { get; set; }
        public TimeSpan StartTime { get; set; }
    }
}

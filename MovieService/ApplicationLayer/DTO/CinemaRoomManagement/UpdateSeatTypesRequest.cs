using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationLayer.DTO.CinemaRoomManagement
{
    public class UpdateSeatTypesRequest
    {
        public Guid RoomId { get; set; }
        public List<UpdateSeatTypeDto> Updates { get; set; } = new();
    }
}

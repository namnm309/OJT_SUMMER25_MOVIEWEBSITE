using DomainLayer.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationLayer.DTO.CinemaRoomManagement
{
    public class UpdateSeatTypeDto
    {
        public Guid SeatId { get; set; }
        public SeatType NewSeatType { get; set; }
    }
}

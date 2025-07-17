using DomainLayer.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationLayer.DTO.CinemaRoomManagement
{
    public class SeatDetailDto
    {
        public Guid Id { get; set; }
        public string SeatCode { get; set; } = null!;
        public SeatType SeatType { get; set; }
        public int RowIndex { get; set; }
        public int ColumnIndex { get; set; }
        public SeatStatus Status { get; set; }
    }
}

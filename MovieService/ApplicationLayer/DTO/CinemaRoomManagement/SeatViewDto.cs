using DomainLayer.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationLayer.DTO.CinemaRoomManagement
{
    public class SeatViewDto
    {
        public Guid Id { get; set; }
        public string SeatCode { get; set; } = string.Empty;
        public SeatType SeatType { get; set; }
        public int RowIndex { get; set; }
        public int ColumnIndex { get; set; }
    }
}

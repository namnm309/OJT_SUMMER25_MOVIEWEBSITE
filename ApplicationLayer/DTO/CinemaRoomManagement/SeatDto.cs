using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationLayer.DTO.CinemaRoomManagement
{ // tôi là PhucAn,
  // tôi đã tạo và code file này đừng gộp code của tôi với code của người khác nhé
  // vì tôi sẽ không hiểu và không đọc được đâu (I'am a beginner)

    public class SeatDto
    {
        public Guid Id { get; set; }
        public string SeatCode { get; set; }
        public string SeatType { get; set; }
        public int RowIndex { get; set; }
        public int ColumnIndex { get; set; }
        public bool IsAvailable { get; set; }
        public decimal Price { get; set; }
    }
    // tôi là PhucAn,
    // tôi đã tạo và code file này đừng gộp code của tôi với code của người khác nhé
    // vì tôi sẽ không hiểu và không đọc được đâu (I'am a beginner)
}

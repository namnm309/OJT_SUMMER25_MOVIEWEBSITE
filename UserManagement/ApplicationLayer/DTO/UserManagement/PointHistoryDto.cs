using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationLayer.DTO.PromotionManagement
{
    public class PointHistoryDto
    {
        public DateTime CreatedAt { get; set; }
        //public string MovieName { get; set; } = string.Empty;
        public double Points { get; set; } // âm nếu là dùng điểm
    }

}

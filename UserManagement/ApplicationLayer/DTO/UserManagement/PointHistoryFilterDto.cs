using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationLayer.DTO.PromotionManagement
{
    public class PointHistoryFilterDto
    {
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public bool? IsUsed { get; set; } // true = dùng điểm, false = cộng điểm
    }

}

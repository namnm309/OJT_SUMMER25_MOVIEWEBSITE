using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationLayer.DTO.Payment
{
    public class PaymentResponseDto
    {
        public bool Success { get; set; }
        public string PaymentMethod { get; set; }
        public string OrderDescription { get; set; }
        public string OrderId { get; set; }
        public string TransactionId { get; set; }
        public string Token { get; set; }
        public string VnPayResponseCode { get; set; }

        public string BookingCode { get; set; }
        
        // Thêm thông tin user role để phân biệt user vs admin
        public string UserRole { get; set; } = string.Empty;
        
        // Thêm thông tin nguồn tạo booking để phân biệt admin dashboard vs user thường
        public string BookingSource { get; set; } = string.Empty;
    }
}

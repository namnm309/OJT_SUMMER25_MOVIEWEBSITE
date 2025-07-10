using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationLayer.DTO.Payment
{
    public class PaymentRequestDto
    {
        public Guid BookingId { get; set; }
        public decimal Amount { get; set; }
        public string Decription { get; set; }
    }
}

using DomainLayer.Enum;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainLayer.Entities
{
    public class Transaction : BaseEntity
    {
        [Required]
        public Guid BookingId { get; set; }

        [Required]
        public Guid UserId { get; set; }

        [Required]
        public decimal Amount { get; set; }

        [Required]
        public string PaymentMethod { get; set; }

        [Required]
        public PaymentStatusEnum PaymentStatus { get; set; }

        [Required]
        public string MerchantTransactionId { get; set; }

        public string? Description { get; set; }

        // Navigation
        [ForeignKey("BookingId")]
        public virtual Booking Booking { get; set; } = null!;

    }
}

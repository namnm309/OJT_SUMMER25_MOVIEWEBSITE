using DomainLayer.Enum;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DomainLayer.Entities
{
    [Table("tbl_seat_log")]
    public class SeatLog : BaseEntity
    {
        [Required]
        public Guid ShowTimeId { get; set; }

        [Required]
        public Guid UserId { get; set; }

        [Required]
        public DateTime ExpiredAt { get; set; }

        [Required]
        public SeatStatus Status { get; set; } = SeatStatus.Pending;

        // Navigation Properties
        public virtual ICollection<SeatLogDetail> SeatLogDetails { get; set; } = new List<SeatLogDetail>();

        [ForeignKey("ShowTimeId")]
        public virtual ShowTime ShowTime { get; set; }

        [ForeignKey("UserId")]
        public virtual Users User { get; set; }
    }
}

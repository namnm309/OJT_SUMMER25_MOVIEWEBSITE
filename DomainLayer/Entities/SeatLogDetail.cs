using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainLayer.Entities
{
    [Table("tbl_seat_log_detail")]
    public class SeatLogDetail : BaseEntity
    {
        [Required]
        public Guid SeatLogId { get; set; }

        [Required]
        public Guid SeatId { get; set; }

        // Navigation
        [ForeignKey("SeatLogId")]
        public SeatLog SeatLog { get; set; }

        [ForeignKey("SeatId")]
        public Seat Seat { get; set; }
    }
}

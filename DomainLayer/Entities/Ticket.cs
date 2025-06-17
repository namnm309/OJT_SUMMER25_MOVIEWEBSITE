using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainLayer.Entities
{
    [Table("tbl_tickets")]
    public class Ticket : BaseEntity
    {
        [Required]
        [StringLength(100)]
        public string MovieName { get; set; } = string.Empty;

    }
}

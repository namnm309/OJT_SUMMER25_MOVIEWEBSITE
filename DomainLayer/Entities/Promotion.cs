using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DomainLayer.Entities
{
    [Table("tbl_promotions")]
    public class Promotion : BaseEntity
    {     
        [Required]
        [StringLength(100)]
        public string Title { get; set; }
        
        [Required]
        public DateTime StartDate { get; set; }
        
        [Required]
        public DateTime EndDate { get; set; }
        
        [Required]
        [Range(0, 100)]
        public int DiscountPercent { get; set; }

        // New: points required to redeem this promotion. 0 means public (free)
        [Column(TypeName = "double precision")]
        public double RequiredPoints { get; set; } = 0;
        
        [Required]
        public string Description { get; set; }
        
        public string ImageUrl { get; set; }
    }
} 
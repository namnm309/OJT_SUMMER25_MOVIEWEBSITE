using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DomainLayer.Enum;

namespace DomainLayer.Entities
{
    [Table("tbl_show_times")]
    public class ShowTime
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid ShowTimeId { get; set; }

        [Required]
        [Column(TypeName = "date")]
        public DateTime ShowDate { get; set; }

        [Required]
        [Column(TypeName = "time")]
        public TimeSpan StartTime { get; set; }
        
        [Required]
        [Column(TypeName = "time")]
        public TimeSpan EndTime { get; set; }

        [Required]
        public ShowTimeStatus Status { get; set; } = ShowTimeStatus.Available;

        [Required]
        public decimal Price { get; set; }

        // Foreign keys
        [Required]
        public Guid MovieId { get; set; }

        [Required]
        public Guid RoomId { get; set; }

        // Navigation properties
        [ForeignKey("MovieId")]
        public virtual Movie Movie { get; set; } = null!;

        [ForeignKey("RoomId")]
        public virtual CinemaRoom Room { get; set; } = null!;

        public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    }
} 
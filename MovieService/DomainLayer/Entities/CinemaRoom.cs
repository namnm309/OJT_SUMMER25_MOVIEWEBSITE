using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DomainLayer.Entities
{
    [Table("tbl_cinema_rooms")]
    public class CinemaRoom : BaseEntity
    {
        [Required]
        [MaxLength(50)]
        public string RoomName { get; set; } = string.Empty;

        [Required]
        public int TotalSeats { get; set; }
        
        [Required]
        public int NumberOfRows { get; set; }
        
        [Required]
        public int NumberOfColumns { get; set; }
        
        public decimal DefaultSeatPrice { get; set; } = 100000;
        
        public bool IsActive { get; set; } = true;

        // Navigation properties
        public virtual ICollection<Seat> Seats { get; set; } = new List<Seat>();
        public virtual ICollection<ShowTime> ShowTimes { get; set; } = new List<ShowTime>();
    }
} 
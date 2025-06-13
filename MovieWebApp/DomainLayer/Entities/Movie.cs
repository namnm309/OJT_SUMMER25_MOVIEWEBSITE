using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DomainLayer.Enum;

namespace DomainLayer.Entities
{
    [Table("tbl_movies")]
    public class Movie : BaseEntity
    {
        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        public DateTime? ReleaseDate { get; set; }

        [Required]
        public DateTime? EndDate { get; set; }

        [MaxLength(100)]
        public string? ProductionCompany { get; set; }

        [MaxLength(50)]
        public int RunningTime { get; set; }

        public MovieVersion? Version { get; set; }

        [MaxLength(500)]
        public string? Actors { get; set; }

        [MaxLength(100)]
        public string? Director { get; set; }

        [MaxLength(500)]
        public string? TrailerUrl { get; set; }

        [MaxLength(2000)]
        public string? Content { get; set; }

        [Required]
        public MovieStatus Status { get; set; }

        // Navigation properties
        public virtual ICollection<MovieImage> MovieImages { get; set; } = new List<MovieImage>();
        public virtual ICollection<MovieGenre> MovieGenres { get; set; } = new List<MovieGenre>();
        public virtual ICollection<ShowTime> ShowTimes { get; set; } = new List<ShowTime>();
    }
}

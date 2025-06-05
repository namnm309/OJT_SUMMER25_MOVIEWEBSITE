using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DomainLayer.Enum;

namespace DomainLayer.Entities
{
    [Table("tbl_movies")]
    public class Movie
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid MovieId { get; set; }

        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [Column(TypeName = "date")]
        public DateTime? ReleaseDate { get; set; }

        [MaxLength(100)]
        public string? ProductionCompany { get; set; }

        [MaxLength(50)]
        public string? RunningTime { get; set; }

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
        public MovieStatus Status { get; set; } = MovieStatus.ChuaCo;

        // Navigation property - Một movie có nhiều hình ảnh
        public virtual ICollection<MovieImage> MovieImages { get; set; } = new List<MovieImage>();
    }
}

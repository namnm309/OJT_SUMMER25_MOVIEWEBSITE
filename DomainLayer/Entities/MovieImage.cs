using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DomainLayer.Entities
{
    [Table("tbl_movie_images")]
    public class MovieImage : BaseEntity
    {
        [Required]
        public Guid MovieId { get; set; }

        [Required]
        [MaxLength(500)]
        public string ImageUrl { get; set; } = string.Empty;

        [MaxLength(200)]
        public string? Description { get; set; }

        public int DisplayOrder { get; set; } = 1;

        [Required]
        public bool IsPrimary { get; set; } = false;

        // Navigation property
        [ForeignKey("MovieId")]
        public virtual Movie Movie { get; set; } = null!;
    }
} 
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DomainLayer.Entities
{
    [Table("tbl_genres")]
    public class Genre
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid GenreId { get; set; }

        [Required]
        [MaxLength(50)]
        public string GenreName { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }
        
        [Required]
        public bool IsActive { get; set; } = true;
        
        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        [Required]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation property
        public virtual ICollection<MovieGenre> MovieGenres { get; set; } = new List<MovieGenre>();
    }
} 
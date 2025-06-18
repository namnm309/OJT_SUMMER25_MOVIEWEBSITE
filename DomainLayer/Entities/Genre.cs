using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DomainLayer.Entities
{
    [Table("tbl_genres")]
    public class Genre : BaseEntity
    {
        [Required]
        [MaxLength(50)]
        public string GenreName { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }
        
        [Required]
        public bool IsActive { get; set; } = true;

        // Navigation property
        public virtual ICollection<MovieGenre> MovieGenres { get; set; } = new List<MovieGenre>();
    }
} 
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DomainLayer.Entities
{
    [Table("tbl_movie_directors")]
    public class MovieDirector : BaseEntity
    {
        [Required]
        public Guid MovieId { get; set; }

        [Required]
        public Guid DirectorId { get; set; }

        // Navigation properties
        [ForeignKey("MovieId")]
        public virtual Movie Movie { get; set; } = null!;

        [ForeignKey("DirectorId")]
        public virtual Director Director { get; set; } = null!;
    }
} 
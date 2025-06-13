using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DomainLayer.Entities
{
    [Table("tbl_movie_genres")]
    public class MovieGenre
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid MovieGenreId { get; set; }

        [Required]
        public Guid MovieId { get; set; }

        [Required]
        public Guid GenreId { get; set; }

        // Navigation properties
        [ForeignKey("MovieId")]
        public virtual Movie Movie { get; set; } = null!;

        [ForeignKey("GenreId")]
        public virtual Genre Genre { get; set; } = null!;
    }
} 
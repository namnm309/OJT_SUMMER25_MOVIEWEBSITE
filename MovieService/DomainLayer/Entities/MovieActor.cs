using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DomainLayer.Entities
{
    [Table("tbl_movie_actors")]
    public class MovieActor : BaseEntity
    {
        [Required]
        public Guid MovieId { get; set; }

        [Required]
        public Guid ActorId { get; set; }

        // Navigation properties
        [ForeignKey("MovieId")]
        public virtual Movie Movie { get; set; } = null!;

        [ForeignKey("ActorId")]
        public virtual Actor Actor { get; set; } = null!;
    }
} 
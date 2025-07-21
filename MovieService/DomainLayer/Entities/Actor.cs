using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DomainLayer.Entities
{
    [Table("tbl_actors")]
    public class Actor : BaseEntity
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        public bool IsActive { get; set; } = true;

        // Navigation property
        public virtual ICollection<MovieActor> MovieActors { get; set; } = new List<MovieActor>();
    }
} 
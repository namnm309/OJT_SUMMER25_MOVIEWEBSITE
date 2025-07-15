using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DomainLayer.Entities
{
    [Table("tbl_directors")]
    public class Director : BaseEntity
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        public bool IsActive { get; set; } = true;

        // Navigation property
        public virtual ICollection<MovieDirector> MovieDirectors { get; set; } = new List<MovieDirector>();
    }
} 
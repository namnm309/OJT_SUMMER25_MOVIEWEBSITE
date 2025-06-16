using System.ComponentModel.DataAnnotations;

namespace UI.Models
{
    public class MovieCreateViewModel
    {
        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;
        
        public DateTime? ReleaseDate { get; set; }
        
        public DateTime? StartDate { get; set; }
        
        public DateTime? EndDate { get; set; }
        
        [MaxLength(100)]
        public string? ProductionCompany { get; set; }
        
        [MaxLength(50)]
        public string? RunningTime { get; set; }
        
        [MaxLength(500)]
        public string? Actors { get; set; }
        
        [MaxLength(100)]
        public string? Director { get; set; }
        
        [MaxLength(500)]
        public string? TrailerUrl { get; set; }
        
        [MaxLength(2000)]
        public string? Content { get; set; }
        
        public string? Version { get; set; } // 2D, 3D, etc.
        
        public List<string> Genres { get; set; } = new List<string>();
        
        public string? ImageUrl { get; set; }
    }

    public class MovieUpdateViewModel : MovieCreateViewModel
    {
        // Inherits all fields from Create
    }
} 
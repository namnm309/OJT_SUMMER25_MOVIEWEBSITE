using DomainLayer.Enum;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationLayer.DTO.MovieManagement
{
    public class MovieCreateDto
    {
        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        public DateTime ReleaseDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        [MaxLength(500)]
        public string? Actors { get; set; }

        [MaxLength(100)]
        public string? ProductionCompany { get; set; }

        [MaxLength(100)]
        public string? Director { get; set; }

        [Required]
        public int RunningTime { get; set; }  // In minutes

        [Required]
        public MovieVersion Version { get; set; }

        [MaxLength(500)]
        public string? TrailerUrl { get; set; }

        [MaxLength(2000)]
        public string? Content { get; set; }

        [Required]
        public List<Guid> GenreIds { get; set; } = new();

        [Required]
        public List<ShowTimeDto> ShowTimes { get; set; } = new();

        [Required]
        public List<MovieImageDto> Images { get; set; } = new();

        // New properties for movie management
        public bool IsRecommended { get; set; } = false;
        public bool IsFeatured { get; set; } = false;
        public double Rating { get; set; } = 0.0;
    }

}

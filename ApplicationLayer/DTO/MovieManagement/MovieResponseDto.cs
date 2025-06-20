using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationLayer.DTO.MovieManagement
{
    public class MovieResponseDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public DateTime ReleaseDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? ProductionCompany { get; set; }
        public int RunningTime { get; set; }
        public string Version { get; set; } = string.Empty;
        public string? Director { get; set; }
        public string? Actors { get; set; }
        public string? Content { get; set; }
        public string? TrailerUrl { get; set; }
        public int Status { get; set; }
        
        // Thêm các properties mới
        public string? PrimaryImageUrl { get; set; }
        public List<MovieImageDto> Images { get; set; } = new();
        public List<GenreDto> Genres { get; set; } = new();
    }
}

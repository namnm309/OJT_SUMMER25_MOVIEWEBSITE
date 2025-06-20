
using DomainLayer.Enum;

namespace ApplicationLayer.DTO.MovieManagement
{
    public class MovieListDto
    {
            public Guid Id { get; set; }
            public string Title { get; set; } = string.Empty;
            public DateTime? ReleaseDate { get; set; }
            public string? ProductionCompany { get; set; }
            public string? RunningTime { get; set; }
            public MovieVersion? Version { get; set; }
    }
}

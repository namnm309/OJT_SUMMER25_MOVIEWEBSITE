using System;

namespace ApplicationLayer.DTO.MovieManagement
{
    public class GenreDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
    }
} 
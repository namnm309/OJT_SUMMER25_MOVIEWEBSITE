using System.Text.Json.Serialization;

namespace UI.Models
{
    public class MovieViewModel
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;
        
        [JsonPropertyName("title")]
        public string Title { get; set; } = string.Empty;
        
        [JsonPropertyName("releaseDate")]
        public DateTime ReleaseDate { get; set; }
        
        [JsonPropertyName("endDate")]
        public DateTime? EndDate { get; set; }
        
        [JsonPropertyName("productionCompany")]
        public string? ProductionCompany { get; set; }

        [JsonPropertyName("runningTime")]
        public int RunningTime { get; set; } = 0;

        [JsonPropertyName("version")]
        public string Version { get; set; } = string.Empty;
        
        [JsonPropertyName("director")]
        public string? Director { get; set; }
        
        [JsonPropertyName("actors")]
        public string? Actors { get; set; }
        
        [JsonPropertyName("content")]
        public string? Content { get; set; }
        
        [JsonPropertyName("trailerUrl")]
        public string? TrailerUrl { get; set; }
        
        [JsonPropertyName("imageUrl")]
        public string? ImageUrl { get; set; }
        
        [JsonPropertyName("primaryImageUrl")]
        public string? PrimaryImageUrl { get; set; }
        
        [JsonPropertyName("images")]
        public List<MovieImageViewModel> Images { get; set; } = new List<MovieImageViewModel>();
        
        [JsonPropertyName("status")]
        public int Status { get; set; }
        
        [JsonPropertyName("genres")]
        public List<MovieGenreViewModel> Genres { get; set; } = new List<MovieGenreViewModel>();
        
        [JsonPropertyName("showTimes")]
        public List<MovieShowTimeViewModel> ShowTimes { get; set; } = new List<MovieShowTimeViewModel>();
        
        // Helper property để lấy tên genres dưới dạng string list
        public List<string> GenreNames => Genres?.Select(g => g.Name).ToList() ?? new List<string>();
    }

    public class MovieShowTimeViewModel
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;
        
        [JsonPropertyName("showDate")]
        public DateTime ShowDate { get; set; }
        
        [JsonPropertyName("showTime")]
        public string ShowTime { get; set; } = string.Empty;
        
        [JsonPropertyName("roomId")]
        public string RoomId { get; set; } = string.Empty;
        
        [JsonPropertyName("roomName")]
        public string RoomName { get; set; } = string.Empty;
        
        [JsonPropertyName("status")]
        public string Status { get; set; } = string.Empty;
    }

    public class MovieImageViewModel
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;
        
        [JsonPropertyName("imageUrl")]
        public string ImageUrl { get; set; } = string.Empty;
        
        [JsonPropertyName("description")]
        public string Description { get; set; } = string.Empty;
        
        [JsonPropertyName("isPrimary")]
        public bool IsPrimary { get; set; }
        
        [JsonPropertyName("displayOrder")]
        public int DisplayOrder { get; set; }
    }

    public class MovieGenreViewModel
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;
        
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;
        
        [JsonPropertyName("description")]
        public string Description { get; set; } = string.Empty;
    }
}

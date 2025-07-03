using System.ComponentModel.DataAnnotations;

namespace UI.Areas.MovieManagement.Models
{
    public class MovieCreateViewModel
    {
        [Required(ErrorMessage = "Tên phim là bắt buộc")]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Ngày phát hành là bắt buộc")]
        public DateTime ReleaseDate { get; set; }
        
        [Required(ErrorMessage = "Ngày kết thúc là bắt buộc")]
        public DateTime EndDate { get; set; }
        
        [MaxLength(100)]
        public string? ProductionCompany { get; set; }
        
        [Required(ErrorMessage = "Thời lượng phim là bắt buộc")]
        [Range(1, 500, ErrorMessage = "Thời lượng phải từ 1 đến 500 phút")]
        public int RunningTime { get; set; }
        
        [MaxLength(500)]
        public string? Actors { get; set; }
        
        [MaxLength(100)]
        public string? Director { get; set; }
        
        [MaxLength(500)]
        public string? TrailerUrl { get; set; }
        
        [MaxLength(2000)]
        public string? Content { get; set; }
        
        [Required(ErrorMessage = "Phiên bản phim là bắt buộc")]
        public string Version { get; set; } = string.Empty; // 2D, 3D
        
        [Range(0.0, 10.0, ErrorMessage = "Điểm đánh giá phải từ 0.0 đến 10.0")]
        public double Rating { get; set; } = 0.0;
        
        public bool IsFeatured { get; set; } = false;
        
        public bool IsRecommended { get; set; } = false;
        
        public List<Guid> GenreIds { get; set; } = new List<Guid>();
        
        public List<string> Genres { get; set; } = new List<string>();
        
        [Required(ErrorMessage = "Hình ảnh phim là bắt buộc")]
        public string ImageUrl { get; set; } = string.Empty;
        
        public List<ShowTimeViewModel> ShowTimes { get; set; } = new List<ShowTimeViewModel>();
        
        public List<MovieImageViewModel> Images { get; set; } = new List<MovieImageViewModel>();
    }

    public class ShowTimeViewModel
    {
        [Required]
        public Guid RoomId { get; set; }
        
        [Required]
        public DateTime ShowDate { get; set; }
    }

    public class MovieImageViewModel
    {
        [Required]
        public string ImageUrl { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsPrimary { get; set; }
    }

    public class MovieUpdateViewModel : MovieCreateViewModel
    {
        public Guid Id { get; set; }
    }
    
    public class GenreViewModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
    }
    
    public class CinemaRoomViewModel  
    {
        public Guid Id { get; set; }
        public string RoomName { get; set; } = string.Empty;
        public int TotalSeats { get; set; }
        public bool IsActive { get; set; }
    }

    public class MovieDisplayViewModel
    {
        public Guid MovieId { get; set; }
        public string Title { get; set; } = string.Empty;
        public DateTime? ReleaseDate { get; set; }
        public int Duration { get; set; }
        public string? PosterUrl { get; set; }
        public double? Rating { get; set; }
        public string Status { get; set; } = string.Empty;
        public List<GenreViewModel> Genres { get; set; } = new List<GenreViewModel>();
    }

    public class MovieIndexViewModel
    {
        public IEnumerable<MovieDisplayViewModel>? Movies { get; set; }
        public IEnumerable<GenreViewModel>? Genres { get; set; }
        public string? SearchTerm { get; set; }
        public Guid? GenreId { get; set; }
        public string? Status { get; set; }
        public int TotalMovies { get; set; }
        public int ActiveMovies { get; set; }
        public int ComingSoonMovies { get; set; }
        public int StoppedMovies { get; set; }
    }
} 
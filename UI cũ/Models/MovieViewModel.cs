namespace UI.Models
{
    public class MovieViewModel
    {
        public string Id { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public DateTime ReleaseDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? ProductionCompany { get; set; }
        public int RunningTime { get; set; }
        public string Version { get; set; } = string.Empty;
        
        // Bổ sung thêm các thuộc tính
        public string? Director { get; set; }
        public string? Actors { get; set; }
        public string? Content { get; set; }
        public string? TrailerUrl { get; set; }
        public string? ImageUrl { get; set; }
        public string? PrimaryImageUrl { get; set; }
        public string? Background { get; set; } // Background image for hero section
        public int Status { get; set; } // 0 = NotAvailable, 1 = Available, 2 = ComingSoon, 3 = Stopped
        
        // Thêm các properties missing
        public double Rating { get; set; } = 0.0;
        public bool IsFeatured { get; set; } = false;
        public bool IsRecommended { get; set; } = false;
        
        public List<GenreViewModel> Genres { get; set; } = new List<GenreViewModel>();
        public List<MovieShowTimeViewModel> ShowTimes { get; set; } = new List<MovieShowTimeViewModel>();
        public List<MovieImageViewModel> Images { get; set; } = new List<MovieImageViewModel>();
    }
    
    public class MovieShowTimeViewModel
    {
        public string Id { get; set; } = string.Empty;
        public DateTime ShowDate { get; set; }
        public string ShowTime { get; set; } = string.Empty;
        public string RoomId { get; set; } = string.Empty;
        public string RoomName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
    }
    
    public class MovieImageViewModel
    {
        public string Id { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool IsPrimary { get; set; }
        public int DisplayOrder { get; set; }
    }
    
    public class GenreViewModel
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
    }

    public class MovieSearchFilterViewModel
    {
        public string? Keyword { get; set; }
        public string? Status { get; set; } = "all"; // all, recommended, coming-soon, now-showing
        public string? Genre { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public double? MinRating { get; set; }
        public double? MaxRating { get; set; }
        public int? MinDuration { get; set; }
        public int? MaxDuration { get; set; }
        public string SortBy { get; set; } = "releaseDate"; // title, releaseDate, rating, duration, director, status
        public string SortOrder { get; set; } = "desc"; // asc, desc
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 12;
        
        // Additional filter options
        public bool? IsFeatured { get; set; }
        public bool? IsRecommended { get; set; }
        public string? Director { get; set; }
        public string? ProductionCompany { get; set; }
        public int? Year { get; set; }
    }

    public class MovieStatisticsViewModel
    {
        public int TotalMovies { get; set; }
        public int NowShowingMovies { get; set; }
        public int ComingSoonMovies { get; set; }
        public int StoppedMovies { get; set; }
        public int FeaturedMovies { get; set; }
        public int RecommendedMovies { get; set; }
        public double AverageRating { get; set; }
        public List<MovieViewModel> TopRatedMovies { get; set; } = new List<MovieViewModel>();
        public List<MovieViewModel> RecentMovies { get; set; } = new List<MovieViewModel>();
        public List<GenreStatsViewModel> GenreStats { get; set; } = new List<GenreStatsViewModel>();
    }

    public class GenreStatsViewModel
    {
        public string GenreName { get; set; } = string.Empty;
        public int MovieCount { get; set; }
        public double AverageRating { get; set; }
    }

    public class MovieFilterOptionsViewModel
    {
        public List<GenreViewModel> Genres { get; set; } = new List<GenreViewModel>();
        public List<string> Directors { get; set; } = new List<string>();
        public List<string> ProductionCompanies { get; set; } = new List<string>();
        public List<int> Years { get; set; } = new List<int>();
        public int MinYear { get; set; }
        public int MaxYear { get; set; }
        public double MinRating { get; set; }
        public double MaxRating { get; set; }
        public int MinDuration { get; set; }
        public int MaxDuration { get; set; }
    }
}

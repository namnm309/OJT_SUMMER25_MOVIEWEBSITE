namespace UI.Models
{
    public class HomeViewModel
    {
        public MovieViewModel? FeaturedMovie { get; set; }
        public List<MovieViewModel> RecommendedMovies { get; set; } = new List<MovieViewModel>();
        public List<MovieViewModel> HeroMovies { get; set; } = new List<MovieViewModel>(); // Danh sách phim cho hero slider (max 5)
        public List<MovieViewModel> ComingSoonMovies { get; set; } = new List<MovieViewModel>(); // Phim sắp chiếu
        public List<PromotionViewModel> Promotions { get; set; } = new List<PromotionViewModel>(); // Khuyến mãi
    }
} 
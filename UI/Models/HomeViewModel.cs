namespace UI.Models
{
    public class HomeViewModel
    {
        public MovieViewModel? FeaturedMovie { get; set; }
        public List<MovieViewModel> RecommendedMovies { get; set; } = new List<MovieViewModel>();
        public List<MovieViewModel> HeroMovies { get; set; } = new List<MovieViewModel>(); // Phim hiển thị trong hero slider (tối đa 5 phim)
        public List<MovieViewModel> ComingSoonMovies { get; set; } = new List<MovieViewModel>(); // Danh sách phim sắp chiếu
        public List<PromotionViewModel> Promotions { get; set; } = new List<PromotionViewModel>(); // Danh sách khuyến mãi hiện tại
    }
} 
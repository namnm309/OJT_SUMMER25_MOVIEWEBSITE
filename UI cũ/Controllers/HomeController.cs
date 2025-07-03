using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using UI.Models;
using UI.Services;
using System.Text.Json;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace UI.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IApiService _apiService;

        public HomeController(ILogger<HomeController> logger, IApiService apiService)
        {
            _logger = logger;
            _apiService = apiService;
        }

        public async Task<IActionResult> Index()
        {
            var viewModel = new HomeViewModel();
            
            try
            {
                // Load initial data with pagination - Medium Dynamic
                await LoadInitialData(viewModel);
                
                // Set promotions data
                SetPromotionsData(viewModel);
                
                _logger.LogInformation("✅ HOMEPAGE LOADED with Medium Dynamic Pagination");
                _logger.LogInformation("🎬 Hero movies: {Count}", viewModel.HeroMovies?.Count ?? 0);
                _logger.LogInformation("⭐ Recommended movies: {Count}", viewModel.RecommendedMovies?.Count ?? 0);
                _logger.LogInformation("📅 Coming soon movies: {Count}", viewModel.ComingSoonMovies?.Count ?? 0);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy dữ liệu phim cho trang chủ - falling back to sample data");
                // Sử dụng dữ liệu mẫu khi có lỗi
                SetFallbackData(viewModel);
                SetPromotionsData(viewModel);
            }
            
            return View(viewModel);
        }

        private void SetFallbackData(HomeViewModel viewModel)
        {
            var fallbackMovies = new List<MovieViewModel>
            {
                new MovieViewModel
                {
                    Id = "1",
                    Title = "Oppenheimer",
                    Director = "Christopher Nolan",
                    Content = "Câu chuyện về J. Robert Oppenheimer, nhà vật lý lý thuyết người Mỹ được mệnh danh là 'cha đẻ của bom nguyên tử'.",
                    RunningTime = 180,
                    ReleaseDate = new DateTime(2023, 7, 21),
                    Rating = 8.4f,
                    Status = 2, // Đang chiếu
                    IsFeatured = true, // Phim nổi bật
                    IsRecommended = true, // Phim đề xuất
                    Genres = new List<GenreViewModel> 
                    { 
                        new GenreViewModel { Id = "1", Name = "Lịch sử" }, 
                        new GenreViewModel { Id = "2", Name = "Tiểu sử" }, 
                        new GenreViewModel { Id = "3", Name = "Chính kịch" } 
                    },
                    PrimaryImageUrl = "https://image.tmdb.org/t/p/original/8Gxv8gSFCU0XGDykEGv7zR1n2ua.jpg",
                    ImageUrl = "https://image.tmdb.org/t/p/original/8Gxv8gSFCU0XGDykEGv7zR1n2ua.jpg",
                    Background = "https://image.tmdb.org/t/p/original/8Gxv8gSFCU0XGDykEGv7zR1n2ua.jpg",
                    TrailerUrl = "https://www.youtube.com/watch?v=uYPbbksJxIg" // Oppenheimer official trailer
                },
                new MovieViewModel
                {
                    Id = "2",
                    Title = "Avatar: The Way of Water",
                    Director = "James Cameron",
                    Content = "Jake Sully sống cùng gia đình mới của mình trên hành tinh Pandora. Khi một mối đe dọa quen thuộc trở lại...",
                    RunningTime = 192,
                    ReleaseDate = new DateTime(2022, 12, 16),
                    Rating = 7.6f,
                    Status = 1, // ComingSoon - sắp chiếu
                    IsFeatured = false,
                    IsRecommended = true, // Phim đề xuất
                    Genres = new List<GenreViewModel> 
                    { 
                        new GenreViewModel { Id = "4", Name = "Hành động" }, 
                        new GenreViewModel { Id = "5", Name = "Phiêu lưu" }, 
                        new GenreViewModel { Id = "6", Name = "Khoa học viễn tưởng" } 
                    },
                    PrimaryImageUrl = "https://image.tmdb.org/t/p/original/t6HIqrRAclMCA60NsSmeqe9RmNV.jpg",
                    ImageUrl = "https://image.tmdb.org/t/p/original/t6HIqrRAclMCA60NsSmeqe9RmNV.jpg",
                    Background = "https://image.tmdb.org/t/p/original/t6HIqrRAclMCA60NsSmeqe9RmNV.jpg",
                    TrailerUrl = "https://www.youtube.com/watch?v=d9MyW72ELq0" // Avatar 2 official trailer
                },
                new MovieViewModel
                {
                    Id = "3",
                    Title = "Spider-Man: No Way Home",
                    Director = "Jon Watts",
                    Content = "Peter Parker đối mặt với đa vũ trụ và các kẻ thù từ những chiều không gian khác...",
                    RunningTime = 148,
                    ReleaseDate = new DateTime(2021, 12, 17),
                    Rating = 8.4f,
                    Status = 2, // NowShowing
                    IsFeatured = true, // Phim nổi bật
                    IsRecommended = false,
                    Genres = new List<GenreViewModel> 
                    { 
                        new GenreViewModel { Id = "4", Name = "Hành động" }, 
                        new GenreViewModel { Id = "5", Name = "Phiêu lưu" }, 
                        new GenreViewModel { Id = "6", Name = "Khoa học viễn tưởng" } 
                    },
                    PrimaryImageUrl = "https://image.tmdb.org/t/p/original/1g0dhYtq4irTY1GPXvft6k4YLjm.jpg",
                    ImageUrl = "https://image.tmdb.org/t/p/original/1g0dhYtq4irTY1GPXvft6k4YLjm.jpg",
                    Background = "https://image.tmdb.org/t/p/original/1g0dhYtq4irTY1GPXvft6k4YLjm.jpg",
                    TrailerUrl = "https://player.cloudinary.com/embed/?cloud_name=swp391image&public_id=fdx0z2h8yz926nq30ys6&profile=cld-default" // Cloudinary embed test
                },
                new MovieViewModel
                {
                    Id = "4",
                    Title = "Dune",
                    Director = "Denis Villeneuve", 
                    Content = "Paul Atreides, một chàng trai thông minh và tài năng sinh ra với số phận vĩ đại...",
                    RunningTime = 155,
                    ReleaseDate = new DateTime(2021, 10, 22),
                    Rating = 8.0f,
                    Status = 1, // Sắp chiếu
                    IsFeatured = true, // Phim nổi bật
                    IsRecommended = true, // Phim đề xuất
                    Genres = new List<GenreViewModel> 
                    { 
                        new GenreViewModel { Id = "4", Name = "Hành động" }, 
                        new GenreViewModel { Id = "5", Name = "Phiêu lưu" }, 
                        new GenreViewModel { Id = "6", Name = "Khoa học viễn tưởng" } 
                    },
                    PrimaryImageUrl = "https://image.tmdb.org/t/p/original/d5NXSklXo0qyIYkgV94XAgMIckC.jpg",
                    ImageUrl = "https://image.tmdb.org/t/p/original/d5NXSklXo0qyIYkgV94XAgMIckC.jpg",
                    Background = "https://image.tmdb.org/t/p/original/d5NXSklXo0qyIYkgV94XAgMIckC.jpg",
                    TrailerUrl = "https://www.youtube.com/watch?v=8g18jFHCLXk" // Dune official trailer
                },
                new MovieViewModel
                {
                    Id = "5",
                    Title = "Interstellar",
                    Director = "Christopher Nolan",
                    Content = "Một nhóm nhà du hành không gian đi qua hố đen để cứu nhân loại.",
                    RunningTime = 169,
                    ReleaseDate = new DateTime(2014, 11, 7),
                    Rating = 8.6f,
                    Status = 2, // NowShowing
                    IsFeatured = false,
                    IsRecommended = true, // Phim đề xuất
                    Genres = new List<GenreViewModel> 
                    { 
                        new GenreViewModel { Id = "6", Name = "Khoa học viễn tưởng" }, 
                        new GenreViewModel { Id = "3", Name = "Chính kịch" } 
                    },
                    PrimaryImageUrl = "https://image.tmdb.org/t/p/original/gEU2QniE6E77NI6lCU6MxlNBvIx.jpg",
                    ImageUrl = "https://image.tmdb.org/t/p/original/gEU2QniE6E77NI6lCU6MxlNBvIx.jpg",
                    Background = "https://image.tmdb.org/t/p/original/gEU2QniE6E77NI6lCU6MxlNBvIx.jpg",
                    TrailerUrl = "https://www.youtube.com/watch?v=zSWdZVtXT7E" // Interstellar official trailer
                }
            };

            // Apply same logic for fallback data
            var featuredMoviesFallback = fallbackMovies.Where(m => m.IsFeatured).ToList();
            viewModel.HeroMovies = featuredMoviesFallback.Any() ? featuredMoviesFallback : fallbackMovies.Take(5).ToList();
            viewModel.FeaturedMovie = viewModel.HeroMovies.FirstOrDefault();
            
            // Phim đề xuất: IsRecommended = true
            viewModel.RecommendedMovies = fallbackMovies.Where(m => m.IsRecommended).ToList();
            
            // Phim sắp chiếu: Status = 1 (ComingSoon)
            viewModel.ComingSoonMovies = fallbackMovies.Where(m => m.Status == 1).ToList();
            
            _logger.LogWarning("🔄 USING FALLBACK DATA for homepage");
            _logger.LogInformation("🎬 Featured movies (Hero): {Count} movies", viewModel.HeroMovies.Count);
            _logger.LogInformation("⭐ Recommended movies: {Count} movies", viewModel.RecommendedMovies.Count);
            _logger.LogInformation("📅 Coming soon movies: {Count} movies", viewModel.ComingSoonMovies.Count);
        }

        private List<GenreViewModel> MapGenres(JsonElement movieJson)
        {
            var genres = new List<GenreViewModel>();
            
            if (movieJson.TryGetProperty("genres", out var genresProp) && genresProp.ValueKind == JsonValueKind.Array)
            {
                foreach (var genreElement in genresProp.EnumerateArray())
                {
                    var genre = new GenreViewModel
                    {
                        Id = genreElement.TryGetProperty("id", out var idProp) ? idProp.GetGuid().ToString() : Guid.NewGuid().ToString(),
                        Name = genreElement.TryGetProperty("name", out var nameProp) ? nameProp.GetString() ?? string.Empty : string.Empty,
                        Description = genreElement.TryGetProperty("description", out var descProp) ? descProp.GetString() : null
                    };
                    
                    if (!string.IsNullOrEmpty(genre.Name))
                    {
                        genres.Add(genre);
                    }
                }
            }
            
            return genres;
        }

        private List<MovieImageViewModel> MapImages(JsonElement movieJson)
        {
            var images = new List<MovieImageViewModel>();
            
            if (movieJson.TryGetProperty("images", out var imagesProp) && imagesProp.ValueKind == JsonValueKind.Array)
            {
                foreach (var imageElement in imagesProp.EnumerateArray())
                {
                    var image = new MovieImageViewModel
                    {
                        Id = Guid.NewGuid().ToString(), // Generate ID vì BE không có
                        ImageUrl = imageElement.TryGetProperty("imageUrl", out var urlProp) ? urlProp.GetString() ?? string.Empty : string.Empty,
                        Description = imageElement.TryGetProperty("description", out var descProp) ? descProp.GetString() ?? string.Empty : string.Empty,
                        IsPrimary = imageElement.TryGetProperty("isPrimary", out var primaryProp) ? primaryProp.GetBoolean() : false,
                        DisplayOrder = imageElement.TryGetProperty("displayOrder", out var orderProp) ? orderProp.GetInt32() : 1
                    };
                    
                    images.Add(image);
                }
            }
            
                        return images;
        }

        private void SetPromotionsData(HomeViewModel viewModel)
        {
            viewModel.Promotions = new List<PromotionViewModel>
            {
                new PromotionViewModel
                {
                    Id = "1",
                    Title = "Giảm 50% vé phim cuối tuần",
                    StartDate = DateTime.Now.AddDays(-7),
                    EndDate = DateTime.Now.AddDays(14),
                    DiscountPercent = 50,
                    Description = "Áp dụng cho tất cả suất chiếu từ thứ 6 đến chủ nhật. Không áp dụng cho phim đặc biệt.",
                    ImageUrl = "https://iguov8nhvyobj.vcdn.cloud/media/wysiwyg/2025/062025/N_O_240x201_1_.png"
                },
                new PromotionViewModel
                {
                    Id = "2", 
                    Title = "Combo bắp nước chỉ 99K",
                    StartDate = DateTime.Now.AddDays(-3),
                    EndDate = DateTime.Now.AddDays(21),
                    DiscountPercent = 30,
                    Description = "Combo bắp rang bơ lớn + 2 nước ngọt + kẹo. Tiết kiệm đến 30% so với giá gốc.",
                    ImageUrl = "https://iguov8nhvyobj.vcdn.cloud/media/wysiwyg/2025/062025/240x201-cj-k-festa.jpg"
                },
                new PromotionViewModel
                {
                    Id = "3",
                    Title = "Thành viên VIP - Ưu đãi đặc biệt",
                    StartDate = DateTime.Now.AddDays(-10),
                    EndDate = DateTime.Now.AddDays(30),
                    DiscountPercent = 25,
                    Description = "Đăng ký thành viên VIP nhận ngay 25% giảm giá cho 3 vé đầu tiên và nhiều ưu đãi hấp dẫn khác.",
                    ImageUrl = "https://iguov8nhvyobj.vcdn.cloud/media/wysiwyg/2025/062025/240x201.png"
                },
                new PromotionViewModel
                {
                    Id = "4",
                    Title = "Sinh viên giảm 40%",
                    StartDate = DateTime.Now.AddDays(-5),
                    EndDate = DateTime.Now.AddDays(60),
                    DiscountPercent = 40,
                    Description = "Dành cho sinh viên có thẻ. Áp dụng từ thứ 2 đến thứ 5, suất chiếu trước 18h.",
                    ImageUrl = "https://iguov8nhvyobj.vcdn.cloud/media/wysiwyg/2025/052025/MMP_Promote_240x201.jpg"
                }
            };
        }

    public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        #region Medium Dynamic Pagination Methods

        private async Task LoadInitialData(HomeViewModel viewModel)
        {
            // Load Hero Movies (Featured) - Page 1, 5 items
            await LoadHeroMovies(viewModel, page: 1, pageSize: 5);
            
            // Load Recommended Movies - Page 1, 6 items  
            await LoadRecommendedMovies(viewModel, page: 1, pageSize: 6);
            
            // Load Coming Soon Movies - Page 1, 4 items
            await LoadComingSoonMovies(viewModel, page: 1, pageSize: 4);
        }

        private async Task LoadHeroMovies(HomeViewModel viewModel, int page, int pageSize)
        {
            try
            {
                var apiUrl = $"/api/v1/movie/ViewPagination?page={page}&pageSize={pageSize}";
                var result = await _apiService.GetAsync<JsonElement>(apiUrl);
                
                if (result.Success && result.Data.TryGetProperty("data", out var paginationData))
                {
                    var data = JsonSerializer.Deserialize<JsonElement>(paginationData.GetRawText());
                    var moviesArray = data.GetProperty("data");
                    var movies = JsonSerializer.Deserialize<List<MovieViewModel>>(moviesArray.GetRawText(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    
                    // Filter featured movies
                    var featuredMovies = movies?.Where(m => m.IsFeatured).ToList() ?? new List<MovieViewModel>();
                    viewModel.HeroMovies = featuredMovies.Any() ? featuredMovies : movies?.Take(5).ToList() ?? new List<MovieViewModel>();
                    viewModel.FeaturedMovie = viewModel.HeroMovies.FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading hero movies");
                viewModel.HeroMovies = new List<MovieViewModel>();
            }
        }

        private async Task LoadRecommendedMovies(HomeViewModel viewModel, int page, int pageSize)
        {
            try
            {
                var result = await _apiService.GetAsync<JsonElement>("/api/v1/movie/GetRecommended");
                
                if (result.Success && result.Data.TryGetProperty("data", out var dataProp))
                {
                    var movies = JsonSerializer.Deserialize<List<MovieViewModel>>(dataProp.GetRawText(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    viewModel.RecommendedMovies = movies?.Take(pageSize).ToList() ?? new List<MovieViewModel>();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading recommended movies");
                viewModel.RecommendedMovies = new List<MovieViewModel>();
            }
        }

        private async Task LoadComingSoonMovies(HomeViewModel viewModel, int page, int pageSize)
        {
            try
            {
                var result = await _apiService.GetAsync<JsonElement>("/api/v1/movie/GetComingSoon");
                
                if (result.Success && result.Data.TryGetProperty("data", out var dataProp))
                {
                    var movies = JsonSerializer.Deserialize<List<MovieViewModel>>(dataProp.GetRawText(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    viewModel.ComingSoonMovies = movies?.Take(pageSize).ToList() ?? new List<MovieViewModel>();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading coming soon movies");
                viewModel.ComingSoonMovies = new List<MovieViewModel>();
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetRecommendedMovies(
            int page = 1, 
            int pageSize = 6,
            string sortBy = "createdAt",
            string sortOrder = "desc",
            string genre = null)
        {
            try
            {
                var apiUrl = "/api/v1/movie/GetRecommended";
                var result = await _apiService.GetAsync<JsonElement>(apiUrl);
                
                if (result.Success && result.Data.TryGetProperty("data", out var dataProp))
                {
                    var allMovies = JsonSerializer.Deserialize<List<MovieViewModel>>(dataProp.GetRawText(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    
                    // Apply genre filter if provided
                    if (!string.IsNullOrEmpty(genre) && genre != "all")
                    {
                        allMovies = allMovies?.Where(m => m.Genres?.Any(g => g.Name.ToLower().Contains(genre.ToLower())) == true).ToList();
                    }
                    
                    // Apply sorting
                    allMovies = ApplySorting(allMovies, sortBy, sortOrder);
                    
                    // Apply pagination
                    var totalItems = allMovies?.Count ?? 0;
                    var totalPages = (int)Math.Ceiling((double)totalItems / pageSize);
                    var skip = (page - 1) * pageSize;
                    var pagedMovies = allMovies?.Skip(skip).Take(pageSize).ToList() ?? new List<MovieViewModel>();
                    
                    return Json(new {
                        success = true,
                        data = pagedMovies,
                        pagination = new {
                            currentPage = page,
                            totalPages = totalPages,
                            totalItems = totalItems,
                            pageSize = pageSize,
                            hasNextPage = page < totalPages,
                            hasPreviousPage = page > 1
                        }
                    });
                }
                
                return Json(new { success = false, message = "Không thể lấy phim đề xuất" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetRecommendedMovies");
                return Json(new { success = false, message = "Lỗi server" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetComingSoonMovies(
            int page = 1,
            int pageSize = 4, 
            string sortBy = "releaseDate",
            string sortOrder = "asc",
            string genre = null,
            DateTime? fromDate = null,
            DateTime? toDate = null)
        {
            try
            {
                var apiUrl = "/api/v1/movie/GetComingSoon";
                var result = await _apiService.GetAsync<JsonElement>(apiUrl);
                
                if (result.Success && result.Data.TryGetProperty("data", out var dataProp))
                {
                    var allMovies = JsonSerializer.Deserialize<List<MovieViewModel>>(dataProp.GetRawText(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    
                    // Apply filters
                    allMovies = ApplyFilters(allMovies, genre, fromDate, toDate);
                    
                    // Apply sorting
                    allMovies = ApplySorting(allMovies, sortBy, sortOrder);
                    
                    // Apply pagination
                    var totalItems = allMovies?.Count ?? 0;
                    var totalPages = (int)Math.Ceiling((double)totalItems / pageSize);
                    var skip = (page - 1) * pageSize;
                    var pagedMovies = allMovies?.Skip(skip).Take(pageSize).ToList() ?? new List<MovieViewModel>();
                    
                    return Json(new {
                        success = true,
                        data = pagedMovies,
                        pagination = new {
                            currentPage = page,
                            totalPages = totalPages,
                            totalItems = totalItems,
                            pageSize = pageSize,
                            hasNextPage = page < totalPages,
                            hasPreviousPage = page > 1
                        }
                    });
                }
                
                return Json(new { success = false, message = "Không thể lấy phim sắp chiếu" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetComingSoonMovies");
                return Json(new { success = false, message = "Lỗi server" });
            }
        }

        private List<MovieViewModel> ApplyFilters(List<MovieViewModel> movies, string genre, DateTime? fromDate, DateTime? toDate)
        {
            if (movies == null) return new List<MovieViewModel>();
            
            // Genre filter
            if (!string.IsNullOrEmpty(genre) && genre != "all")
            {
                movies = movies.Where(m => m.Genres?.Any(g => g.Name.ToLower().Contains(genre.ToLower())) == true).ToList();
            }
            
            // Date range filter
            if (fromDate.HasValue)
            {
                movies = movies.Where(m => m.ReleaseDate >= fromDate.Value).ToList();
            }
            
            if (toDate.HasValue)
            {
                movies = movies.Where(m => m.ReleaseDate <= toDate.Value).ToList();
            }
            
            return movies;
        }

        private List<MovieViewModel> ApplySorting(List<MovieViewModel> movies, string sortBy, string sortOrder)
        {
            if (movies == null) return new List<MovieViewModel>();
            
            var orderedMovies = sortBy.ToLower() switch
            {
                "title" => sortOrder.ToLower() == "desc" 
                    ? movies.OrderByDescending(m => m.Title)
                    : movies.OrderBy(m => m.Title),
                "releasedate" => sortOrder.ToLower() == "desc"
                    ? movies.OrderByDescending(m => m.ReleaseDate)
                    : movies.OrderBy(m => m.ReleaseDate),
                "rating" => sortOrder.ToLower() == "desc"
                    ? movies.OrderByDescending(m => m.Rating)
                    : movies.OrderBy(m => m.Rating),
                _ => sortOrder.ToLower() == "desc"
                    ? movies.OrderByDescending(m => m.ReleaseDate)
                    : movies.OrderBy(m => m.ReleaseDate)
            };
            
            return orderedMovies.ToList();
        }

        #endregion
    }
}

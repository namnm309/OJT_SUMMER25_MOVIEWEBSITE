using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using UI.Models;
using UI.Services;

namespace UI.Controllers
{
    public class MoviesController : Controller
    {
        private readonly IApiService _apiService;
        private readonly ILogger<MoviesController> _logger;

        public MoviesController(IApiService apiService, ILogger<MoviesController> logger)
        {
            _apiService = apiService;
            _logger = logger;
        }

        // Thêm hàm mới để xử lý lấy danh sách phim
        public async Task<IActionResult> MovieManagement()
        {
            ViewData["Title"] = "Quản lý phim";

            try
            {
                var result = await _apiService.GetAsync<JsonElement>("/api/v1/movie/View");

                if (result.Success && result.Data.ValueKind != JsonValueKind.Undefined)
                {
                    if (result.Data.TryGetProperty("data", out var dataProp))
                    {
                        var options = new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        };

                        var movies = JsonSerializer.Deserialize<List<MovieViewModel>>(dataProp.GetRawText(), options);

                        // Log để debug
                        _logger.LogInformation("Received {Count} movies", movies?.Count);
                        foreach (var movie in movies ?? new List<MovieViewModel>())
                        {
                            _logger.LogInformation("Movie: {Title} - {Date}", movie.Title, movie.ReleaseDate);
                        }

                        return View("MovieManagement", movies);
                    }
                }

                _logger.LogError("Failed to get movies: {Message}", result.Message);
                TempData["ErrorMessage"] = result.Message;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting movies");
                TempData["ErrorMessage"] = "Đã xảy ra lỗi khi tải danh sách phim";
            }

            return View("MovieManagement", new List<MovieViewModel>());
        }

        public IActionResult Index()
        {
            ViewData["Title"] = "Phim";
            return View();
        }

        public IActionResult Details(int id)
        {
            ViewData["Title"] = "Chi tiết phim";
            ViewData["MovieId"] = id;
            return View();
        }

        public IActionResult Showtimes(int id)
        {
            ViewData["Title"] = "Lịch chiếu";
            ViewData["MovieId"] = id;
            return View();
        }
    }
}

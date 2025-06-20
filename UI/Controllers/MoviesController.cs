using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using UI.Models;
using UI.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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

        public async Task<IActionResult> Index()
        {
            ViewData["Title"] = "Phim";
            
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
                        
                        // Parse thành dynamic object trước để xử lý mapping
                        var moviesJson = JsonSerializer.Deserialize<JsonElement[]>(dataProp.GetRawText(), options);
                        var movies = new List<MovieViewModel>();
                        
                        if (moviesJson != null)
                        {
                            foreach (var movieJson in moviesJson)
                            {
                                var movie = MapJsonToMovieViewModel(movieJson);
                                movies.Add(movie);
                            }
                        }
                        
                        return View(movies);
                    }
                }
                
                _logger.LogError("Không thể lấy danh sách phim: {Message}", result.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách phim");
            }
            
            return View(new List<MovieViewModel>());
        }

        public async Task<IActionResult> Details(Guid id)
        {
            ViewData["Title"] = "Chi tiết phim";
            ViewData["MovieId"] = id;
            
            try
            {
                var result = await _apiService.GetAsync<JsonElement>($"/api/v1/movie/GetById?movieId={id}");
                
                if (result.Success && result.Data.ValueKind != JsonValueKind.Undefined)
                {
                    if (result.Data.TryGetProperty("data", out var dataProp))
                    {
                        var movie = MapJsonToMovieViewModel(dataProp);
                        return View(movie);
                    }
                }
                
                _logger.LogError("Không thể lấy chi tiết phim: {Message}", result.Message);
                TempData["ErrorMessage"] = "Không thể lấy thông tin chi tiết phim";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy chi tiết phim");
                TempData["ErrorMessage"] = "Đã xảy ra lỗi khi tải thông tin chi tiết phim";
            }
            
            return View(new MovieViewModel());
        }

        public IActionResult Showtimes(Guid id)
        {
            ViewData["Title"] = "Lịch chiếu";
            ViewData["MovieId"] = id;
            return View();
        }
    }
}

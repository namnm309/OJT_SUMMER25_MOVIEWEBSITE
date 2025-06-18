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
                // Lấy danh sách phim từ API
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
                        
                        if (movies != null && movies.Any())
                        {
                            // Lấy tối đa 5 phim cho hero slider
                            viewModel.HeroMovies = movies.Take(5).ToList();
                            viewModel.FeaturedMovie = viewModel.HeroMovies.FirstOrDefault();
                            viewModel.RecommendedMovies = movies.Take(6).ToList();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy dữ liệu phim cho trang chủ");
            }
            
            return View(viewModel);
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
    }
}

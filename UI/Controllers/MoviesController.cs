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

        public IActionResult Index(string filter = "all", int page = 1)
        {
            ViewData["Title"] = "Phim";
            ViewData["CurrentFilter"] = filter;
            ViewData["CurrentPage"] = page;
            
            try
            {
                // Khởi tạo danh sách rỗng để không bị lỗi khi load trang đầu tiên
                return View(new List<MovieViewModel>());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi khởi tạo trang phim");
                return View(new List<MovieViewModel>());
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetMoviesByFilter(string filter = "all", int page = 1, int pageSize = 12)
        {
            try
            {
                string apiUrl;
                
                switch (filter.ToLower())
                {
                    case "all":
                        // Sử dụng ViewPagination API cho "Tất cả phim"
                        apiUrl = $"/api/v1/movie/ViewPagination?page={page}&pageSize={pageSize}";
                        break;
                        
                    case "recommended":
                        // Lấy TẤT CẢ phim đề xuất (không phân trang)
                        apiUrl = "/api/v1/movie/GetRecommended";
                        break;
                        
                    case "coming-soon":
                        // Lấy TẤT CẢ phim sắp chiếu (không phân trang) 
                        apiUrl = "/api/v1/movie/GetComingSoon";
                        break;
                        
                    case "now-showing":
                        // Lấy TẤT CẢ phim đang chiếu (không phân trang)
                        apiUrl = "/api/v1/movie/GetNowShowing";
                        break;
                        
                    default:
                        apiUrl = $"/api/v1/movie/ViewPagination?page={page}&pageSize={pageSize}";
                        break;
                }

                var result = await _apiService.GetAsync<JsonElement>(apiUrl);
                
                _logger.LogInformation("API Response for filter {Filter}: Success={Success}", filter, result.Success);
                
                if (result.Success && result.Data.ValueKind != JsonValueKind.Undefined)
                {
                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    };

                    if (filter == "all")
                    {
                        try
                        {
                            // Parse pagination response: result.Data contains the SuccessResp structure
                            if (result.Data.TryGetProperty("data", out var paginationDataProp))
                            {
                                var paginationData = JsonSerializer.Deserialize<JsonElement>(paginationDataProp.GetRawText(), options);
                                
                                // Extract movies array and pagination info
                                var moviesArray = paginationData.GetProperty("data");
                                var movies = JsonSerializer.Deserialize<List<MovieViewModel>>(moviesArray.GetRawText(), options);
                                
                                var total = paginationData.GetProperty("total").GetInt32();
                                var currentPage = paginationData.GetProperty("page").GetInt32();
                                var currentPageSize = paginationData.GetProperty("pageSize").GetInt32();
                                var totalPages = (int)Math.Ceiling((double)total / currentPageSize);
                                
                                _logger.LogInformation("Successfully loaded {Count} movies for page {Page}", movies?.Count ?? 0, currentPage);
                                
                                return Json(new { 
                                    success = true, 
                                    data = movies,
                                    pagination = new {
                                        currentPage = currentPage,
                                        totalPages = totalPages,
                                        totalItems = total,
                                        pageSize = currentPageSize
                                    }
                                });
                            }
                            
                            return Json(new { success = false, message = "Invalid pagination response structure" });
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Error parsing pagination response: {Data}", result.Data.GetRawText());
                            return Json(new { success = false, message = $"Parse error: {ex.Message}" });
                        }
                    }
                    else
                    {
                        // For non-paginated responses (recommended, coming-soon, now-showing)
                        if (result.Data.TryGetProperty("data", out var dataProp))
                        {
                            var movies = JsonSerializer.Deserialize<List<MovieViewModel>>(dataProp.GetRawText(), options);
                            return Json(new { 
                                success = true, 
                                data = movies,
                                totalCount = movies?.Count ?? 0
                            });
                        }
                    }
                }
                
                _logger.LogError("Không thể lấy danh sách phim với filter {Filter}: {Message}", filter, result.Message);
                return Json(new { success = false, message = result.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách phim với filter {Filter}", filter);
                return Json(new { success = false, message = "Đã xảy ra lỗi khi tải danh sách phim" });
            }
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
                        var options = new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        };
                        
                        var movie = JsonSerializer.Deserialize<MovieViewModel>(dataProp.GetRawText(), options);
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

        public async Task<IActionResult> Search(string keyword)
        {
            ViewData["Title"] = string.IsNullOrEmpty(keyword) ? "Tất cả phim" : $"Kết quả tìm kiếm: {keyword}";
            ViewData["SearchKeyword"] = keyword;

            try
            {
                // Gọi API search - nếu không có keyword thì hiển thị tất cả
                var apiUrl = string.IsNullOrEmpty(keyword)
                    ? "/api/v1/movie/View"
                    : $"/api/v1/movie/Search?keyword={Uri.EscapeDataString(keyword)}";

                var result = await _apiService.GetAsync<JsonElement>(apiUrl);

                if (result.Success && result.Data.ValueKind != JsonValueKind.Undefined)
                {
                    if (result.Data.TryGetProperty("data", out var dataProp))
                    {
                        var options = new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        };

                        var movies = JsonSerializer.Deserialize<List<MovieViewModel>>(dataProp.GetRawText(), options);
                        return View("SearchResults", movies);
                    }
                }

                _logger.LogError("Không thể tìm kiếm phim: {Message}", result.Message);
                TempData["ErrorMessage"] = "Không thể tìm kiếm phim";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tìm kiếm phim");
                TempData["ErrorMessage"] = "Đã xảy ra lỗi khi tìm kiếm";
            }

            return View("SearchResults", new List<MovieViewModel>());
        }
    }
}

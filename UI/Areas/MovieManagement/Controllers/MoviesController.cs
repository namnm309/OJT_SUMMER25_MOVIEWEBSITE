using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using UI.Models;
using UI.Services;
using Microsoft.AspNetCore.Authorization;
using UI.Areas.MovieManagement.Models;
using Microsoft.Extensions.Configuration;

namespace UI.Areas.MovieManagement.Controllers
{
    [Area("MovieManagement")]
    [Authorize(Roles = "Admin,Staff")] // Chỉ Admin và Staff mới được quản lý phim
    public class MoviesController : Controller
    {
        private readonly IApiService _apiService;
        private readonly ILogger<MoviesController> _logger;

        public MoviesController(IApiService apiService, ILogger<MoviesController> logger)
        {
            _apiService = apiService;
            _logger = logger;
        }

        // Action để hiển thị trang quản lý phim
        public async Task<IActionResult> Index()
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

                        _logger.LogInformation("Nhận được {Count} phim", movies?.Count);
                        
                        return View(movies);
                    }
                }

                _logger.LogError("Không thể lấy danh sách phim: {Message}", result.Message);
                TempData["ErrorMessage"] = result.Message;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách phim");
                TempData["ErrorMessage"] = "Đã xảy ra lỗi khi tải danh sách phim";
            }

            return View(new List<MovieViewModel>());
        }
        
        [HttpGet]
        public IActionResult Create()
        {
            ViewData["Title"] = "Thêm phim mới";
            
            // Get API base URL from configuration
            var configuration = HttpContext.RequestServices.GetRequiredService<IConfiguration>();
            ViewBag.ApiBaseUrl = configuration["ApiSettings:BaseUrl"] ?? "http://localhost:5274";
            
            return View(new MovieCreateViewModel());
        }
        
        [HttpPost]
        public async Task<IActionResult> Create(MovieCreateViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);
                
            try
            {
                // Chuyển đổi từ MovieCreateViewModel sang MovieCreateDto
                var movieDto = new
                {
                    Title = model.Title,
                    ReleaseDate = model.ReleaseDate,
                    EndDate = model.EndDate,
                    Actors = model.Actors,
                    ProductionCompany = model.ProductionCompany,
                    Director = model.Director,
                    RunningTime = model.RunningTime,
                    Version = model.Version == "2D" ? 0 : 1, // 0 = TwoD, 1 = ThreeD
                    TrailerUrl = model.TrailerUrl,
                    Content = model.Content,
                    GenreIds = model.GenreIds, // Sử dụng GenreIds từ form
                    ShowTimes = model.ShowTimes?.Select(st => new {
                        RoomId = st.RoomId,
                        ShowDate = st.ShowDate
                    } as object).ToList() ?? new List<object>(),
                    Images = model.Images?.Select(img => new {
                        ImageUrl = img.ImageUrl,
                        IsPrimary = img.IsPrimary,
                        Description = img.Description ?? "",
                        DisplayOrder = img.DisplayOrder
                    } as object).ToList() ?? new List<object>()
                };
                
                var result = await _apiService.PostAsync<JsonElement>("/api/v1/movie/Create", movieDto);
                
                if (result.Success)
                {
                    TempData["SuccessMessage"] = "Thêm phim mới thành công";
                    return RedirectToAction("Index");
                }
                
                ModelState.AddModelError("", result.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tạo phim mới");
                ModelState.AddModelError("", "Đã xảy ra lỗi khi tạo phim mới");
            }
            
            return View(model);
        }
        
        [HttpGet]
        public async Task<IActionResult> Edit(Guid id)
        {
            ViewData["Title"] = "Chỉnh sửa phim";
            
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
                        
                        if (movie != null)
                        {
                            var updateModel = new MovieUpdateViewModel
                            {
                                Title = movie.Title,
                                ReleaseDate = movie.ReleaseDate,
                                ProductionCompany = movie.ProductionCompany,
                                RunningTime = movie.RunningTime,
                                Version = movie.Version
                                // Các trường khác cần được điền từ movie
                            };
                            
                            return View(updateModel);
                        }
                    }
                }
                
                TempData["ErrorMessage"] = "Không thể lấy thông tin phim để chỉnh sửa";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy thông tin phim để chỉnh sửa");
                TempData["ErrorMessage"] = "Đã xảy ra lỗi khi tải thông tin phim";
            }
            
            return RedirectToAction("Index");
        }
        
        [HttpPost]
        public async Task<IActionResult> Edit(Guid id, MovieUpdateViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);
                
            try
            {
                // Chuyển đổi từ MovieUpdateViewModel sang MovieUpdateDto
                var movieDto = new
                {
                    Id = id,
                    Title = model.Title,
                    ReleaseDate = model.ReleaseDate,
                    EndDate = model.EndDate,
                    Actors = model.Actors,
                    ProductionCompany = model.ProductionCompany,
                    Director = model.Director,
                    RunningTime = model.RunningTime,
                    Version = model.Version == "2D" ? 0 : 1, // 0 = TwoD, 1 = ThreeD
                    TrailerUrl = model.TrailerUrl,
                    Content = model.Content,
                    GenreIds = model.Genres?.Select(g => Guid.Parse(g)).ToList() ?? new List<Guid>(),
                    ShowTimes = new List<object>(), // Cần bổ sung thông tin lịch chiếu
                    Images = new List<object>() // Cần bổ sung thông tin hình ảnh
                    {
                        new {
                            ImageUrl = model.ImageUrl,
                            IsPrimary = true,
                            Description = model.Title,
                            DisplayOrder = 1
                        }
                    }
                };
                
                var result = await _apiService.PutAsync<JsonElement>("/api/v1/movie/Update", movieDto);
                
                if (result.Success)
                {
                    TempData["SuccessMessage"] = "Cập nhật phim thành công";
                    return RedirectToAction("Index");
                }
                
                ModelState.AddModelError("", result.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi cập nhật phim");
                ModelState.AddModelError("", "Đã xảy ra lỗi khi cập nhật phim");
            }
            
            return View(model);
        }
        
        [HttpPost]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                var result = await _apiService.DeleteAsync($"/api/v1/movie/Delete?Id={id}");
                
                if (result.Success)
                {
                    TempData["SuccessMessage"] = "Xóa phim thành công";
                }
                else
                {
                    TempData["ErrorMessage"] = result.Message;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xóa phim");
                TempData["ErrorMessage"] = "Đã xảy ra lỗi khi xóa phim";
            }
            
            return RedirectToAction("Index");
        }
        
        [HttpPost]
        public async Task<IActionResult> ChangeStatus(Guid id, int status)
        {
            try
            {
                var result = await _apiService.GetAsync<JsonElement>($"/api/v1/movie/ChangeStatus?Id={id}&Status={status}");
                
                if (result.Success)
                {
                    TempData["SuccessMessage"] = "Thay đổi trạng thái phim thành công";
                }
                else
                {
                    TempData["ErrorMessage"] = result.Message;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi thay đổi trạng thái phim");
                TempData["ErrorMessage"] = "Đã xảy ra lỗi khi thay đổi trạng thái phim";
            }
            
            return RedirectToAction("Index");
        }
        
        [HttpGet]
        public async Task<IActionResult> GetGenres()
        {
            try
            {
                var result = await _apiService.GetAsync<JsonElement>("/api/v1/movie/genres");
                return Json(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách thể loại");
                return Json(new { success = false, message = "Không thể tải danh sách thể loại" });
            }
        }
        
        [HttpGet]
        public async Task<IActionResult> GetCinemaRooms()
        {
            try
            {
                var result = await _apiService.GetAsync<JsonElement>("/api/v1/movie/cinemarooms");
                return Json(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách phòng chiếu");
                return Json(new { success = false, message = "Không thể tải danh sách phòng chiếu" });
            }
        }
    }
} 
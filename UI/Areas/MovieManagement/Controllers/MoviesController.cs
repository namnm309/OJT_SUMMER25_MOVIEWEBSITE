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
                        var movies = new List<MovieViewModel>();
                        
                        if (dataProp.ValueKind == JsonValueKind.Array)
                        {
                            foreach (var movieElement in dataProp.EnumerateArray())
                            {
                                var movie = MapJsonToMovieViewModel(movieElement);
                                if (movie != null)
                                {
                                    movies.Add(movie);
                                }
                            }
                        }

                        _logger.LogInformation("Nhận được {Count} phim", movies.Count);
                        
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
                return Json(new { success = false, message = "Dữ liệu không hợp lệ" });
                
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
                    Images = new List<object>
                    {
                        new {
                            ImageUrl = model.ImageUrl,
                            IsPrimary = true,
                            Description = model.Title,
                            DisplayOrder = 1
                        }
                    }
                };
                
                var result = await _apiService.PostAsync<JsonElement>("/api/v1/movie/Create", movieDto);
                
                if (result.Success)
                {
                    return Json(new { success = true, message = "Thêm phim mới thành công" });
                }
                
                return Json(new { success = false, message = result.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tạo phim mới");
                return Json(new { success = false, message = "Đã xảy ra lỗi khi tạo phim mới" });
            }
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
                return Json(new { success = false, message = "Dữ liệu không hợp lệ" });
                
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
                
                var result = await _apiService.PostAsync<JsonElement>("/api/v1/movie/Update", movieDto);
                
                if (result.Success)
                {
                    return Json(new { success = true, message = "Cập nhật phim thành công" });
                }
                
                return Json(new { success = false, message = result.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi cập nhật phim");
                return Json(new { success = false, message = "Đã xảy ra lỗi khi cập nhật phim" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> UpdateMovie([FromBody] JsonElement movieData)
        {
            try
            {
                _logger.LogInformation("Received movie data: {Data}", movieData.GetRawText());
                
                // Parse JSON data
                var movieDto = JsonSerializer.Deserialize<JsonElement>(movieData.GetRawText());
                
                // Forward to BE API - BE sử dụng PATCH method
                var result = await _apiService.PatchAsync<JsonElement>("/api/v1/movie/Update", movieDto);
                
                if (result.Success)
                {
                    return Json(new { success = true, message = "Cập nhật phim thành công" });
                }
                
                return Json(new { success = false, message = result.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi cập nhật phim: {Error}", ex.Message);
                return Json(new { success = false, message = "Đã xảy ra lỗi khi cập nhật phim" });
            }
        }
        
        [HttpPost]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                var result = await _apiService.DeleteAsync($"/api/v1/movie/Delete?Id={id}");
                
                if (result.Success)
                {
                    return Json(new { success = true, message = "Xóa phim thành công" });
                }
                else
                {
                    return Json(new { success = false, message = result.Message });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xóa phim");
                return Json(new { success = false, message = "Đã xảy ra lỗi khi xóa phim" });
            }
        }
        
        [HttpPost]
        public async Task<IActionResult> ChangeStatus(Guid id, int status)
        {
            try
            {
                var result = await _apiService.GetAsync<JsonElement>($"/api/v1/movie/ChangeStatus?Id={id}&Status={status}");
                
                if (result.Success)
                {
                    return Json(new { success = true, message = "Thay đổi trạng thái phim thành công" });
                }
                else
                {
                    return Json(new { success = false, message = result.Message });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi thay đổi trạng thái phim");
                return Json(new { success = false, message = "Đã xảy ra lỗi khi thay đổi trạng thái phim" });
            }
        }
        
        [HttpGet]
        public async Task<IActionResult> GetMovie(Guid id)
        {
            try
            {
                var result = await _apiService.GetAsync<JsonElement>($"/api/v1/movie/GetById?movieId={id}");
                
                if (result.Success && result.Data.ValueKind != JsonValueKind.Undefined)
                {
                    if (result.Data.TryGetProperty("data", out var dataProp))
                    {
                        // Trả về raw JSON data thay vì mapping
                        var rawData = JsonSerializer.Deserialize<object>(dataProp.GetRawText());
                        return Json(new { success = true, data = rawData });
                    }
                }
                
                return Json(new { success = false, message = "Không thể tải thông tin phim" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy thông tin phim");
                return Json(new { success = false, message = "Đã xảy ra lỗi khi tải thông tin phim" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetMoviesJson()
        {
            try
            {
                var result = await _apiService.GetAsync<JsonElement>("/api/v1/movie/View");

                if (result.Success && result.Data.ValueKind != JsonValueKind.Undefined)
                {
                    if (result.Data.TryGetProperty("data", out var dataProp))
                    {
                        var movies = new List<MovieViewModel>();
                        
                        if (dataProp.ValueKind == JsonValueKind.Array)
                        {
                            foreach (var movieElement in dataProp.EnumerateArray())
                            {
                                var movie = MapJsonToMovieViewModel(movieElement);
                                if (movie != null)
                                {
                                    movies.Add(movie);
                                }
                            }
                        }

                        return Json(new { success = true, data = movies });
                    }
                }

                return Json(new { success = false, message = result.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách phim");
                return Json(new { success = false, message = "Đã xảy ra lỗi khi tải danh sách phim" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetGenres()
        {
            try
            {
                var result = await _apiService.GetAsync<JsonElement>("/api/v1/movie/ViewGenre");
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
                var result = await _apiService.GetAsync<JsonElement>("/api/v1/cinemaroom/ViewRoom");
                return Json(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách phòng chiếu");
                return Json(new { success = false, message = "Không thể tải danh sách phòng chiếu" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetStatistics()
        {
            try
            {
                // Tạm thời return mock data, sau này có thể tạo API riêng cho statistics
                var moviesResult = await _apiService.GetAsync<JsonElement>("/api/v1/movie/View");
                
                if (moviesResult.Success && moviesResult.Data.ValueKind != JsonValueKind.Undefined)
                {
                    if (moviesResult.Data.TryGetProperty("data", out var dataProp))
                    {
                        var movies = new List<MovieViewModel>();
                        
                        if (dataProp.ValueKind == JsonValueKind.Array)
                        {
                            foreach (var movieElement in dataProp.EnumerateArray())
                            {
                                var movie = MapJsonToMovieViewModel(movieElement);
                                if (movie != null)
                                {
                                    movies.Add(movie);
                                }
                            }
                        }
                        
                        var stats = new
                        {
                            totalMovies = movies.Count,
                            activeMovies = movies.Count(m => m.Status == 1),  // 1 = Đang chiếu
                            comingMovies = movies.Count(m => m.Status == 2),  // 2 = Sắp chiếu
                            totalRevenue = 0 // Mock data
                        };
                        
                        return Json(new { success = true, data = stats });
                    }
                }
                
                return Json(new { success = false, message = "Không thể tải thống kê" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy thống kê");
                return Json(new { success = false, message = "Đã xảy ra lỗi khi tải thống kê" });
            }
        }

        // Helper method to map JSON to MovieViewModel
        private MovieViewModel? MapJsonToMovieViewModel(JsonElement movieElement)
        {
            try
            {
                var movie = new MovieViewModel();

                // Basic properties - try both camelCase and PascalCase
                if (movieElement.TryGetProperty("id", out var idProp) || movieElement.TryGetProperty("Id", out idProp))
                    movie.Id = idProp.GetString() ?? "";

                if (movieElement.TryGetProperty("title", out var titleProp) || movieElement.TryGetProperty("Title", out titleProp))
                    movie.Title = titleProp.GetString() ?? "";

                if (movieElement.TryGetProperty("releaseDate", out var releaseProp) || movieElement.TryGetProperty("ReleaseDate", out releaseProp))
                {
                    DateTime.TryParse(releaseProp.GetString(), out var releaseDate);
                    movie.ReleaseDate = releaseDate;
                }

                if (movieElement.TryGetProperty("endDate", out var endDateProp) || movieElement.TryGetProperty("EndDate", out endDateProp))
                {
                    DateTime.TryParse(endDateProp.GetString(), out var endDate);
                    movie.EndDate = endDate;
                }

                if (movieElement.TryGetProperty("productionCompany", out var companyProp) || movieElement.TryGetProperty("ProductionCompany", out companyProp))
                    movie.ProductionCompany = companyProp.GetString();

                if (movieElement.TryGetProperty("runningTime", out var timeProp) || movieElement.TryGetProperty("RunningTime", out timeProp))
                    movie.RunningTime = timeProp.GetInt32();

                if (movieElement.TryGetProperty("version", out var versionProp) || movieElement.TryGetProperty("Version", out versionProp))
                    movie.Version = versionProp.GetString() ?? "";

                if (movieElement.TryGetProperty("director", out var directorProp) || movieElement.TryGetProperty("Director", out directorProp))
                    movie.Director = directorProp.GetString();

                if (movieElement.TryGetProperty("actors", out var actorsProp) || movieElement.TryGetProperty("Actors", out actorsProp))
                    movie.Actors = actorsProp.GetString();

                if (movieElement.TryGetProperty("content", out var contentProp) || movieElement.TryGetProperty("Content", out contentProp))
                    movie.Content = contentProp.GetString();

                if (movieElement.TryGetProperty("trailerUrl", out var trailerProp) || movieElement.TryGetProperty("TrailerUrl", out trailerProp))
                    movie.TrailerUrl = trailerProp.GetString();

                if (movieElement.TryGetProperty("status", out var statusProp) || movieElement.TryGetProperty("Status", out statusProp))
                    movie.Status = statusProp.GetInt32();

                // Handle Images - try both property names
                JsonElement imagesProp;
                var hasImages = movieElement.TryGetProperty("images", out imagesProp) || 
                               movieElement.TryGetProperty("Images", out imagesProp);
                
                if (hasImages && imagesProp.ValueKind == JsonValueKind.Array)
                {
                    var primaryImage = "";
                    foreach (var img in imagesProp.EnumerateArray())
                    {
                        JsonElement imgUrlProp;
                        var hasImageUrl = img.TryGetProperty("imageUrl", out imgUrlProp) || 
                                         img.TryGetProperty("ImageUrl", out imgUrlProp);
                        
                        if (hasImageUrl)
                        {
                            var imageUrl = imgUrlProp.GetString();
                            if (!string.IsNullOrEmpty(imageUrl))
                            {
                                JsonElement isPrimaryProp;
                                var hasIsPrimary = img.TryGetProperty("isPrimary", out isPrimaryProp) || 
                                                  img.TryGetProperty("IsPrimary", out isPrimaryProp);
                                
                                if (hasIsPrimary && isPrimaryProp.GetBoolean())
                                {
                                    primaryImage = imageUrl;
                                    break;
                                }
                                if (string.IsNullOrEmpty(primaryImage))
                                {
                                    primaryImage = imageUrl;
                                }
                            }
                        }
                    }
                    movie.ImageUrl = primaryImage;
                }

                // Handle PrimaryImageUrl if available
                if (movieElement.TryGetProperty("primaryImageUrl", out var primaryUrlProp) || 
                    movieElement.TryGetProperty("PrimaryImageUrl", out primaryUrlProp))
                {
                    var primaryUrl = primaryUrlProp.GetString();
                    if (!string.IsNullOrEmpty(primaryUrl))
                    {
                        movie.ImageUrl = primaryUrl;
                    }
                }

                // Handle Genres
                JsonElement genresProp;
                var hasGenres = movieElement.TryGetProperty("genres", out genresProp) || 
                               movieElement.TryGetProperty("Genres", out genresProp);
                
                if (hasGenres)
                {
                    movie.Genres = MapGenres(genresProp);
                }

                return movie;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error mapping movie JSON: {Json}", movieElement.GetRawText());
                return null;
            }
        }

        private List<string> MapGenres(JsonElement genresElement)
        {
            var genres = new List<string>();
            
            if (genresElement.ValueKind == JsonValueKind.Array)
            {
                foreach (var genre in genresElement.EnumerateArray())
                {
                    if (genre.ValueKind == JsonValueKind.String)
                    {
                        // If it's already a string
                        var genreName = genre.GetString();
                        if (!string.IsNullOrEmpty(genreName))
                        {
                            genres.Add(genreName);
                        }
                    }
                    else if (genre.ValueKind == JsonValueKind.Object)
                    {
                        // If it's an object with name property
                        if (genre.TryGetProperty("name", out var nameProp))
                        {
                            var genreName = nameProp.GetString();
                            if (!string.IsNullOrEmpty(genreName))
                            {
                                genres.Add(genreName);
                            }
                        }
                        // Try other possible property names
                        else if (genre.TryGetProperty("genreName", out var genreNameProp))
                        {
                            var genreName = genreNameProp.GetString();
                            if (!string.IsNullOrEmpty(genreName))
                            {
                                genres.Add(genreName);
                            }
                        }
                    }
                }
            }
            
            return genres;
        }
    }
} 
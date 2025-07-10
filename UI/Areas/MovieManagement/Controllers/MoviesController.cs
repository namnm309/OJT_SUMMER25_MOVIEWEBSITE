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
    [Authorize(Roles = "Admin,Staff")] 
    public class MoviesController : Controller
    {
        private readonly IApiService _apiService;
        private readonly ILogger<MoviesController> _logger;
        private readonly IImageService _imageService;

        public MoviesController(IApiService apiService, ILogger<MoviesController> logger, IImageService imageService)
        {
            _apiService = apiService;
            _logger = logger;
            _imageService = imageService;
        }

        
        public async Task<IActionResult> Index(string? searchTerm, Guid? genreId, string? status)
        {
            ViewData["Title"] = "Quản lý phim";

            try
            {
                // Gọi API để lấy danh sách phim và thể loại từ backend
                var moviesResult = await _apiService.GetAsync<JsonElement>("/api/v1/movie/View");
                var genresResult = await _apiService.GetAsync<JsonElement>("/api/v1/movie/ViewGenre");

                var movieDisplayList = new List<UI.Areas.MovieManagement.Models.MovieDisplayViewModel>();
                var genresList = new List<UI.Areas.MovieManagement.Models.GenreViewModel>();

                // Xử lý dữ liệu phim trả về từ API
                if (moviesResult.Success && moviesResult.Data.ValueKind != JsonValueKind.Undefined)
                {
                    if (moviesResult.Data.TryGetProperty("data", out var dataProp))
                    {
                        if (dataProp.ValueKind == JsonValueKind.Array)
                        {
                            foreach (var movieElement in dataProp.EnumerateArray())
                            {
                                var movieViewModel = MapJsonToMovieViewModel(movieElement);
                                if (movieViewModel != null)
                                {
                                    var movieDisplay = new UI.Areas.MovieManagement.Models.MovieDisplayViewModel
                                    {
                                        MovieId = Guid.TryParse(movieViewModel.Id, out var id) ? id : Guid.Empty,
                                        Title = movieViewModel.Title,
                                        ReleaseDate = movieViewModel.ReleaseDate,
                                        Duration = movieViewModel.RunningTime,
                                        PosterUrl = movieViewModel.ImageUrl,
                                        Rating = movieViewModel.Rating,
                                        Status = movieViewModel.Status switch
                                        {
                                            1 => "Active",
                                            2 => "ComingSoon", 
                                            0 => "Stopped",
                                            _ => "Stopped"
                                        },
                                        Genres = movieViewModel.Genres?.Select(g => new UI.Areas.MovieManagement.Models.GenreViewModel 
                                        { 
                                            Name = g.Name,
                                            Id = Guid.TryParse(g.Id, out var parsedId) ? parsedId : Guid.NewGuid(),
                                            Description = g.Description
                                        }).ToList() ?? new List<UI.Areas.MovieManagement.Models.GenreViewModel>()
                                    };
                                    movieDisplayList.Add(movieDisplay);
                                }
                            }
                        }
                    }
                }

                // Xử lý dữ liệu thể loại phim
                if (genresResult.Success && genresResult.Data.ValueKind != JsonValueKind.Undefined)
                {
                    if (genresResult.Data.TryGetProperty("data", out var genresDataProp))
                    {
                        if (genresDataProp.ValueKind == JsonValueKind.Array)
                        {
                            foreach (var genreElement in genresDataProp.EnumerateArray())
                            {
                                var genre = new UI.Areas.MovieManagement.Models.GenreViewModel();
                                
                                if (genreElement.TryGetProperty("id", out var idProp) || genreElement.TryGetProperty("Id", out idProp))
                                {
                                    if (Guid.TryParse(idProp.GetString(), out var genreId_))
                                        genre.Id = genreId_;
                                }
                                
                                if (genreElement.TryGetProperty("name", out var nameProp) || genreElement.TryGetProperty("Name", out nameProp))
                                    genre.Name = nameProp.GetString() ?? "";
                                
                                if (genreElement.TryGetProperty("description", out var descProp) || genreElement.TryGetProperty("Description", out descProp))
                                    genre.Description = descProp.GetString();

                                genresList.Add(genre);
                            }
                        }
                    }
                }

                // Áp dụng các bộ lọc tìm kiếm
                if (!string.IsNullOrEmpty(searchTerm))
                {
                    movieDisplayList = movieDisplayList
                        .Where(m => m.Title.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
                        .ToList();
                }

                if (genreId.HasValue && genreId != Guid.Empty)
                {
                    movieDisplayList = movieDisplayList
                        .Where(m => m.Genres.Any(g => g.Id == genreId))
                        .ToList();
                }

                if (!string.IsNullOrEmpty(status))
                {
                    movieDisplayList = movieDisplayList
                        .Where(m => m.Status.Equals(status, StringComparison.OrdinalIgnoreCase))
                        .ToList();
                }

                // Tạo ViewModel cuối cùng để trả về cho View
                var viewModel = new UI.Areas.MovieManagement.Models.MovieIndexViewModel
                {
                    Movies = movieDisplayList,
                    Genres = genresList,
                    SearchTerm = searchTerm,
                    GenreId = genreId,
                    Status = status,
                    TotalMovies = movieDisplayList.Count,
                    ActiveMovies = movieDisplayList.Count(m => m.Status == "Active"),
                    ComingSoonMovies = movieDisplayList.Count(m => m.Status == "ComingSoon"),
                    StoppedMovies = movieDisplayList.Count(m => m.Status == "Stopped")
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách phim");
                TempData["ErrorMessage"] = "Đã xảy ra lỗi khi tải danh sách phim";
                
                // Nếu có lỗi thì trả về ViewModel rỗng
                var emptyViewModel = new UI.Areas.MovieManagement.Models.MovieIndexViewModel
                {
                    Movies = new List<UI.Areas.MovieManagement.Models.MovieDisplayViewModel>(),
                    Genres = new List<UI.Areas.MovieManagement.Models.GenreViewModel>(),
                    SearchTerm = searchTerm,
                    GenreId = genreId,
                    Status = status
                };
                
                return View(emptyViewModel);
            }
        }
        
        [HttpGet]
        public IActionResult Create()
        {
            ViewData["Title"] = "Thêm phim mới";
            
            var configuration = HttpContext.RequestServices.GetRequiredService<IConfiguration>();
            ViewBag.ApiBaseUrl = configuration["ApiSettings:BaseUrl"] ?? "http://localhost:5274";
            
            return View(new MovieCreateViewModel());
        }
        
        [HttpPost]
        public async Task<IActionResult> Create(MovieCreateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                // Log validation errors để debug
                var errors = ModelState
                    .Where(x => x.Value.Errors.Count > 0)
                    .Select(x => new { Field = x.Key, Errors = x.Value.Errors.Select(e => e.ErrorMessage) })
                    .ToList();
                
                _logger.LogWarning("ModelState validation failed: {@Errors}", errors);
                
                var errorMessage = string.Join("; ", errors.SelectMany(e => e.Errors));
                return Json(new { success = false, message = $"Dữ liệu không hợp lệ: {errorMessage}" });
            }
                
            try
            {
                // Chuyển đổi dữ liệu từ form thành định dạng mà API có thể hiểu
                var movieDto = new
                {
                    Title = model.Title,
                    ReleaseDate = model.ReleaseDate,
                    EndDate = model.EndDate,
                    Actors = model.Actors,
                    ProductionCompany = model.ProductionCompany,
                    Director = model.Director,
                    RunningTime = model.RunningTime,
                    Version = model.Version, // Trực tiếp sử dụng số từ form
                    TrailerUrl = model.TrailerUrl,
                    Content = model.Content,
                    GenreIds = model.GenreIds, // Danh sách ID thể loại được chọn từ form
                    ShowTimes = model.ShowTimes?.Select(st => new {
                        RoomId = st.RoomId,
                        ShowDate = st.ShowDate
                    } as object).ToList() ?? new List<object>(),
                    Images = new List<object>
                    {
                        new {
                            ImageUrl = model.ImageUrl,
                            IsPrimary = true,
                            Description = model.Title, // Có thể sửa thành description riêng nếu cần
                            DisplayOrder = 1
                        }
                    },
                    // Thêm các field thiếu
                    IsRecommended = model.IsRecommended,
                    IsFeatured = model.IsFeatured,
                    Rating = model.Rating
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
        
        [HttpPost]
        public async Task<IActionResult> CreateMovieAjax([FromBody] JsonElement movieData)
        {
            try
            {
                _logger.LogInformation("Received movie data via AJAX: {MovieData}", movieData);
                
                // Gửi trực tiếp đến API backend
                var result = await _apiService.PostAsync<JsonElement>("/api/v1/movie/Create", movieData);
                
                if (result.Success)
                {
                    return Json(new { success = true, message = "Thêm phim mới thành công" });
                }
                
                return Json(new { success = false, message = result.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tạo phim mới qua AJAX");
                return Json(new { success = false, message = "Đã xảy ra lỗi khi tạo phim mới" });
            }
        }


        [HttpPost]
        public async Task<IActionResult> UpdateMovie([FromBody] JsonElement movieData)
        {
            try
            {

                var movieDto = JsonSerializer.Deserialize<JsonElement>(movieData.GetRawText());
                

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
                var result = await _apiService.PatchAsync<JsonElement>($"/api/v1/movie/ChangeStatus?Id={id}&Status={status}", null);
                
                if (result.Success)
                {
                    return Json(new { success = true, message = "Thay đổi trạng thái phim thành công" });
                }
                else
                {
                    return Json(new { success = false, message = result.Message ?? "Có lỗi xảy ra khi thay đổi trạng thái" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi thay đổi trạng thái phim với ID: {Id}", id);
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
                        // Chuyển đổi dữ liệu JSON thành object để trả về cho frontend
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
        public async Task<IActionResult> GetMoviesPagination(int page = 1, int pageSize = 10, string? searchTerm = null)
        {
            try
            {
                _logger.LogInformation("Getting movies with pagination - Page: {Page}, PageSize: {PageSize}, SearchTerm: {SearchTerm}", 
                    page, pageSize, searchTerm ?? "null");
                
                string apiUrl = $"/api/v1/movie/ViewPagination?Page={page}&PageSize={pageSize}";
                
                // Thêm từ khóa tìm kiếm vào URL nếu có
                if (!string.IsNullOrEmpty(searchTerm))
                {
                    // Mã hóa từ khóa tìm kiếm để tránh lỗi URL
                    apiUrl += $"&searchTerm={Uri.EscapeDataString(searchTerm)}";
                }
                
                var result = await _apiService.GetAsync<JsonElement>(apiUrl);

                if (result.Success && result.Data.ValueKind != JsonValueKind.Undefined)
                {
                    // Phân tích response từ API - có thể có cấu trúc lồng nhau
                    JsonElement dataElement = result.Data;
                    
                    // Kiểm tra xem có thuộc tính "data" trong response không
                    if (result.Data.TryGetProperty("data", out var outerDataProp))
                    {
                        dataElement = outerDataProp;
                        
                        // Nếu đã có cấu trúc phân trang đầy đủ thì trả về luôn
                        if (dataElement.TryGetProperty("data", out var _) && 
                            dataElement.TryGetProperty("total", out var _) && 
                            dataElement.TryGetProperty("page", out var _) && 
                            dataElement.TryGetProperty("pageSize", out var _))
                        {
                            return Json(new { success = true, data = dataElement });
                        }
                        
                        // Kiểm tra cấu trúc dữ liệu sâu hơn một cấp
                        if (dataElement.TryGetProperty("data", out var innerDataProp))
                        {
                            var movies = new List<object>();
                            int totalItems = 0;
                            
                            // Lấy tổng số phim nếu có thông tin này
                            if (dataElement.TryGetProperty("total", out var totalProp))
                            {
                                totalItems = totalProp.GetInt32();
                            }
                            
                            if (innerDataProp.ValueKind == JsonValueKind.Array)
                            {
                                foreach (var movieElement in innerDataProp.EnumerateArray())
                                {
                                    var movie = ExtractMovieData(movieElement);
                                    if (movie != null)
                                    {
                                        movies.Add(movie);
                                    }
                                }
                                
                                // Nếu API không trả về tổng số thì dùng số lượng hiện tại
                                if (totalItems == 0)
                                {
                                    totalItems = movies.Count;
                                }
                            }

                            // Tạo cấu trúc phân trang chuẩn để trả về
                            return Json(new { 
                                success = true, 
                                data = new { 
                                    data = movies, 
                                    total = totalItems,
                                    page = page,
                                    pageSize = pageSize
                                }
                            });
                        }
                    }
                    
                    // Trường hợp dữ liệu là mảng phim trực tiếp
                    if (dataElement.ValueKind == JsonValueKind.Array)
                    {
                        var movies = new List<object>();
                        
                        foreach (var movieElement in dataElement.EnumerateArray())
                        {
                            var movie = ExtractMovieData(movieElement);
                            if (movie != null)
                            {
                                movies.Add(movie);
                            }
                        }

                        // Tạo mới cấu trúc phân trang vì API không có
                        return Json(new { 
                            success = true, 
                            data = new { 
                                data = movies, 
                                total = movies.Count,
                                page = page,
                                pageSize = pageSize
                            }
                        });
                    }
                }

                return Json(new { success = false, message = result.Message ?? "Không thể tải danh sách phim" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách phim pagination");
                return Json(new { success = false, message = "Đã xảy ra lỗi khi tải danh sách phim" });
            }
        }

        private object? ExtractMovieData(JsonElement movieElement)
        {
            try
            {
                var movie = new
                {
                    id = GetJsonProperty(movieElement, "id"),
                    title = GetJsonProperty(movieElement, "title"),
                    releaseDate = GetJsonProperty(movieElement, "releaseDate"),
                    endDate = GetJsonProperty(movieElement, "endDate"),
                    productionCompany = GetJsonProperty(movieElement, "productionCompany"),
                    runningTime = GetJsonPropertyAsInt(movieElement, "runningTime"),
                    director = GetJsonProperty(movieElement, "director"),
                    actors = GetJsonProperty(movieElement, "actors"),
                    content = GetJsonProperty(movieElement, "content"),
                    status = GetJsonPropertyAsInt(movieElement, "status"),
                    rating = GetJsonPropertyAsDouble(movieElement, "rating"),
                    isFeatured = GetJsonPropertyAsBool(movieElement, "isFeatured"),
                    isRecommended = GetJsonPropertyAsBool(movieElement, "isRecommended"),
                    posterUrl = GetPosterUrl(movieElement),
                    genres = GetGenres(movieElement)
                };

                return movie;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error extracting movie data from JSON");
                return null;
            }
        }

        private string GetJsonProperty(JsonElement element, string propertyName)
        {
            // Thử tìm theo các cách viết khác nhau (lowercase, camelCase, PascalCase)
            if (element.TryGetProperty(propertyName.ToLower(), out var prop) ||
                element.TryGetProperty(propertyName, out prop) ||
                element.TryGetProperty(char.ToUpper(propertyName[0]) + propertyName.Substring(1), out prop))
            {
                return prop.GetString() ?? "";
            }
            return "";
        }

        private int GetJsonPropertyAsInt(JsonElement element, string propertyName)
        {
            if (element.TryGetProperty(propertyName.ToLower(), out var prop) ||
                element.TryGetProperty(propertyName, out prop) ||
                element.TryGetProperty(char.ToUpper(propertyName[0]) + propertyName.Substring(1), out prop))
            {
                if (prop.ValueKind == JsonValueKind.Number)
                    return prop.GetInt32();
                if (prop.ValueKind == JsonValueKind.String && int.TryParse(prop.GetString(), out var intVal))
                    return intVal;
            }
            return 0;
        }

        private double GetJsonPropertyAsDouble(JsonElement element, string propertyName)
        {
            if (element.TryGetProperty(propertyName.ToLower(), out var prop) ||
                element.TryGetProperty(propertyName, out prop) ||
                element.TryGetProperty(char.ToUpper(propertyName[0]) + propertyName.Substring(1), out prop))
            {
                if (prop.ValueKind == JsonValueKind.Number)
                    return prop.GetDouble();
                if (prop.ValueKind == JsonValueKind.String && double.TryParse(prop.GetString(), out var doubleVal))
                    return doubleVal;
            }
            return 0.0;
        }

        private bool GetJsonPropertyAsBool(JsonElement element, string propertyName)
        {
            if (element.TryGetProperty(propertyName.ToLower(), out var prop) ||
                element.TryGetProperty(propertyName, out prop) ||
                element.TryGetProperty(char.ToUpper(propertyName[0]) + propertyName.Substring(1), out prop))
            {
                if (prop.ValueKind == JsonValueKind.True) return true;
                if (prop.ValueKind == JsonValueKind.False) return false;
                if (prop.ValueKind == JsonValueKind.String && bool.TryParse(prop.GetString(), out var boolVal))
                    return boolVal;
            }
            return false;
        }

        private string GetPosterUrl(JsonElement movieElement)
        {
            // Ưu tiên lấy ảnh chính của phim
            if (movieElement.TryGetProperty("primaryImageUrl", out var primaryProp) ||
                movieElement.TryGetProperty("PrimaryImageUrl", out primaryProp))
            {
                var url = primaryProp.GetString();
                if (!string.IsNullOrEmpty(url)) return url;
            }

            // Tìm trong danh sách ảnh và ưu tiên ảnh được đánh dấu là chính
            if (movieElement.TryGetProperty("images", out var imagesProp) ||
                movieElement.TryGetProperty("Images", out imagesProp))
            {
                if (imagesProp.ValueKind == JsonValueKind.Array)
                {
                    foreach (var img in imagesProp.EnumerateArray())
                    {
                        if (img.TryGetProperty("isPrimary", out var isPrimaryProp) && isPrimaryProp.GetBoolean())
                        {
                            if (img.TryGetProperty("imageUrl", out var urlProp))
                                return urlProp.GetString() ?? "";
                        }
                    }
                    
                    // Nếu không có ảnh chính thì lấy ảnh đầu tiên
                    if (imagesProp.GetArrayLength() > 0)
                    {
                        var firstImg = imagesProp.EnumerateArray().First();
                        if (firstImg.TryGetProperty("imageUrl", out var urlProp))
                            return urlProp.GetString() ?? "";
                    }
                }
            }

            return "";
        }

        private List<object> GetGenres(JsonElement movieElement)
        {
            var genres = new List<object>();
            
            if (movieElement.TryGetProperty("genres", out var genresProp) ||
                movieElement.TryGetProperty("Genres", out genresProp))
            {
                if (genresProp.ValueKind == JsonValueKind.Array)
                {
                    foreach (var genre in genresProp.EnumerateArray())
                    {
                        if (genre.TryGetProperty("name", out var nameProp) ||
                            genre.TryGetProperty("Name", out nameProp))
                        {
                            var name = nameProp.GetString();
                            if (!string.IsNullOrEmpty(name))
                                genres.Add(new { name = name });
                        }
                    }
                }
            }
            
            return genres;
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
                            activeMovies = movies.Count(m => m.Status == 1),
                            comingMovies = movies.Count(m => m.Status == 2),
                            totalRevenue = 0
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


        [HttpPatch]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SetFeatured(Guid movieId, bool isFeatured)
        {
            try
            {
                var result = await _apiService.PatchAsync<JsonElement>($"/api/v1/movie/SetFeatured?movieId={movieId}&isFeatured={isFeatured}", null);
                
                if (result.Success || result.Data.ValueKind != JsonValueKind.Undefined)
                {
                    return Json(new { success = true, message = $"Đã {(isFeatured ? "thêm vào" : "xóa khỏi")} danh sách phim nổi bật" });
                }
                else
                {
                    return Json(new { success = false, message = result.Message ?? "Có lỗi xảy ra khi cập nhật trạng thái Featured" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi cập nhật trạng thái Featured cho phim {MovieId}", movieId);
                return Json(new { success = false, message = "Đã xảy ra lỗi khi cập nhật trạng thái Featured" });
            }
        }

        [HttpPatch]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SetRecommended(Guid movieId, bool isRecommended)
        {
            try
            {
                var result = await _apiService.PatchAsync<JsonElement>($"/api/v1/movie/SetRecommended?movieId={movieId}&isRecommended={isRecommended}", null);
                
                if (result.Success || result.Data.ValueKind != JsonValueKind.Undefined)
                {
                    return Json(new { success = true, message = $"Đã {(isRecommended ? "thêm vào" : "xóa khỏi")} danh sách phim đề xuất" });
                }
                else
                {
                    return Json(new { success = false, message = result.Message ?? "Có lỗi xảy ra khi cập nhật trạng thái Recommended" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi cập nhật trạng thái Recommended cho phim {MovieId}", movieId);
                return Json(new { success = false, message = "Đã xảy ra lỗi khi cập nhật trạng thái Recommended" });
            }
        }

        [HttpPatch]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangeMovieStatus(Guid movieId, int status)
        {
            try
            {
                var result = await _apiService.PatchAsync<JsonElement>($"/api/v1/movie/ChangeStatus?Id={movieId}&Status={status}", null);
                
                if (result.Success || result.Data.ValueKind != JsonValueKind.Undefined)
                {
                    string statusText = status switch
                    {
                        1 => "Đang chiếu",
                        2 => "Sắp chiếu", 
                        0 => "Ngừng chiếu",
                        _ => "Không xác định"
                    };
                    return Json(new { success = true, message = $"Đã chuyển trạng thái phim thành: {statusText}" });
                }
                else
                {
                    return Json(new { success = false, message = result.Message ?? "Có lỗi xảy ra khi cập nhật trạng thái phim" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi cập nhật trạng thái cho phim {MovieId}", movieId);
                return Json(new { success = false, message = "Đã xảy ra lỗi khi cập nhật trạng thái phim" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetMovieById(Guid movieId)
        {
            try
            {
                var result = await _apiService.GetAsync<JsonElement>($"/api/v1/movie/GetById?movieId={movieId}");
                return Json(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy thông tin phim {MovieId}", movieId);
                return Json(new { success = false, message = "Đã xảy ra lỗi khi tải thông tin phim" });
            }
        }

        [HttpPatch]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateRating(Guid movieId, double rating)
        {
            try
            {
                var result = await _apiService.PatchAsync<JsonElement>($"/api/v1/movie/UpdateRating?movieId={movieId}&rating={rating}", null);
                
                if (result.Success || result.Data.ValueKind != JsonValueKind.Undefined)
                {
                    return Json(new { success = true, message = $"Đã cập nhật rating thành {rating}/10" });
                }
                else
                {
                    return Json(new { success = false, message = result.Message ?? "Có lỗi xảy ra khi cập nhật rating" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi cập nhật rating cho phim {MovieId}", movieId);
                return Json(new { success = false, message = "Đã xảy ra lỗi khi cập nhật rating" });
            }
        }

        [HttpPatch]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateMovieData([FromBody] JsonElement movieData)
        {
            try
            {
                var result = await _apiService.PatchAsync<JsonElement>("/api/v1/movie/Update", movieData);
                
                if (result.Success)
                {
                    return Json(new { success = true, message = "Cập nhật phim thành công!" });
                }
                else
                {
                    return Json(new { success = false, message = result.Message ?? "Có lỗi xảy ra khi cập nhật phim" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi cập nhật phim");
                return Json(new { success = false, message = "Đã xảy ra lỗi khi cập nhật phim" });
            }
        }


        private MovieViewModel? MapJsonToMovieViewModel(JsonElement movieElement)
        {
            try
            {
                var movie = new MovieViewModel()
                {
                    Version = "2D",
                    Status = 0,
                    RunningTime = 0,
                    ReleaseDate = DateTime.Now,
                    Genres = new List<UI.Models.GenreViewModel>()
                };


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
                {
                    movie.Version = versionProp.GetString() ?? "2D";
                }

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


                if (movieElement.TryGetProperty("isFeatured", out var featuredProp) || movieElement.TryGetProperty("IsFeatured", out featuredProp))
                    movie.IsFeatured = featuredProp.GetBoolean();

                if (movieElement.TryGetProperty("isRecommended", out var recommendedProp) || movieElement.TryGetProperty("IsRecommended", out recommendedProp))
                    movie.IsRecommended = recommendedProp.GetBoolean();

                if (movieElement.TryGetProperty("rating", out var ratingProp) || movieElement.TryGetProperty("Rating", out ratingProp))
                    movie.Rating = ratingProp.GetDouble();


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


                if (movieElement.TryGetProperty("primaryImageUrl", out var primaryUrlProp) || 
                    movieElement.TryGetProperty("PrimaryImageUrl", out primaryUrlProp))
                {
                    var primaryUrl = primaryUrlProp.GetString();
                    if (!string.IsNullOrEmpty(primaryUrl))
                    {
                        movie.ImageUrl = primaryUrl;
                    }

                }


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

        private List<UI.Models.GenreViewModel> MapGenres(JsonElement genresElement)
        {
            var genres = new List<UI.Models.GenreViewModel>();
            
            if (genresElement.ValueKind == JsonValueKind.Array)
            {
                foreach (var genre in genresElement.EnumerateArray())
                {
                    if (genre.ValueKind == JsonValueKind.String)
                    {
                        var genreName = genre.GetString();
                        if (!string.IsNullOrEmpty(genreName))
                        {
                            genres.Add(new UI.Models.GenreViewModel { Id = Guid.NewGuid().ToString(), Name = genreName });
                        }
                    }
                    else if (genre.ValueKind == JsonValueKind.Object)
                    {
                        var genreViewModel = new UI.Models.GenreViewModel
                        {
                            Id = genre.TryGetProperty("id", out var idProp) ? idProp.GetGuid().ToString() : Guid.NewGuid().ToString(),
                            Description = genre.TryGetProperty("description", out var descProp) ? descProp.GetString() : null
                        };
                        
                        if (genre.TryGetProperty("name", out var nameProp))
                        {
                            genreViewModel.Name = nameProp.GetString() ?? string.Empty;
                        }
                        else if (genre.TryGetProperty("genreName", out var genreNameProp))
                        {
                            genreViewModel.Name = genreNameProp.GetString() ?? string.Empty;
                        }
                        
                        if (!string.IsNullOrEmpty(genreViewModel.Name))
                        {
                            genres.Add(genreViewModel);
                        }
                    }
                }
            }
            
            return genres;
        }


        [HttpPost]
        public async Task<IActionResult> UploadImage(IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                {
                    return Json(new { success = false, message = "Không có file được chọn" });
                }

                var imageUrl = await _imageService.UploadImageAsync(file);
                return Json(new { success = true, imageUrl = imageUrl });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi upload hình ảnh");
                return Json(new { success = false, message = "Lỗi khi upload hình ảnh: " + ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> UploadVideo(IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                {
                    return Json(new { success = false, message = "Không có file được chọn" });
                }

                var videoUrl = await _imageService.UploadVideoAsync(file);
                return Json(new { success = true, videoUrl = videoUrl });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi upload video");
                return Json(new { success = false, message = "Lỗi khi upload video: " + ex.Message });
            }
        }
    }
} 
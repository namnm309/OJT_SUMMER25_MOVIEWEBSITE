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

        private MovieViewModel MapJsonToMovieViewModel(JsonElement movieJson)
        {
            return new MovieViewModel
            {
                Id = movieJson.TryGetProperty("id", out var idProp) ? idProp.GetGuid().ToString() : string.Empty,
                Title = movieJson.TryGetProperty("title", out var titleProp) ? titleProp.GetString() ?? string.Empty : string.Empty,
                ReleaseDate = movieJson.TryGetProperty("releaseDate", out var releaseProp) ? releaseProp.GetDateTime() : DateTime.Now,
                ProductionCompany = movieJson.TryGetProperty("productionCompany", out var prodProp) ? prodProp.GetString() : null,
                RunningTime = movieJson.TryGetProperty("runningTime", out var timeProp) ? timeProp.GetInt32() : 0,
                Version = movieJson.TryGetProperty("version", out var versionProp) ? versionProp.GetString() ?? string.Empty : string.Empty,
                Director = movieJson.TryGetProperty("director", out var directorProp) ? directorProp.GetString() : null,
                Actors = movieJson.TryGetProperty("actors", out var actorsProp) ? actorsProp.GetString() : null,
                Content = movieJson.TryGetProperty("content", out var contentProp) ? contentProp.GetString() : null,
                TrailerUrl = movieJson.TryGetProperty("trailerUrl", out var trailerProp) ? trailerProp.GetString() : null,
                Status = movieJson.TryGetProperty("status", out var statusProp) ? statusProp.GetInt32() : 0,
                
                // Map PrimaryImageUrl và ImageUrl
                PrimaryImageUrl = movieJson.TryGetProperty("primaryImageUrl", out var primaryImgProp) ? primaryImgProp.GetString() : null,
                ImageUrl = movieJson.TryGetProperty("primaryImageUrl", out var imgProp) ? imgProp.GetString() : null, // Fallback cho ImageUrl
                
                // Map Genres từ List<GenreDto> sang List<string>
                Genres = MapGenres(movieJson),
                
                // Map Images từ List<MovieImageDto> sang List<MovieImageViewModel>
                Images = MapImages(movieJson),
                
                // Map ShowTimes (có thể không có trong response)
                ShowTimes = MapShowTimes(movieJson)
            };
        }

        private List<string> MapGenres(JsonElement movieJson)
        {
            var genres = new List<string>();
            
            if (movieJson.TryGetProperty("genres", out var genresProp) && genresProp.ValueKind == JsonValueKind.Array)
            {
                foreach (var genreElement in genresProp.EnumerateArray())
                {
                    if (genreElement.TryGetProperty("name", out var nameProp))
                    {
                        var genreName = nameProp.GetString();
                        if (!string.IsNullOrEmpty(genreName))
                        {
                            genres.Add(genreName);
                        }
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

        private List<MovieShowTimeViewModel> MapShowTimes(JsonElement movieJson)
        {
            var showTimes = new List<MovieShowTimeViewModel>();
            
            if (movieJson.TryGetProperty("showTimes", out var showTimesProp) && showTimesProp.ValueKind == JsonValueKind.Array)
            {
                foreach (var showTimeElement in showTimesProp.EnumerateArray())
                {
                    var showTime = new MovieShowTimeViewModel
                    {
                        Id = showTimeElement.TryGetProperty("id", out var idProp) ? idProp.GetGuid().ToString() : Guid.NewGuid().ToString(),
                        ShowDate = showTimeElement.TryGetProperty("showDate", out var dateProp) ? dateProp.GetDateTime() : DateTime.Now,
                        ShowTime = showTimeElement.TryGetProperty("showTime", out var timeProp) ? timeProp.GetString() ?? string.Empty : string.Empty,
                        RoomId = showTimeElement.TryGetProperty("roomId", out var roomIdProp) ? roomIdProp.GetGuid().ToString() : string.Empty,
                        RoomName = showTimeElement.TryGetProperty("roomName", out var roomNameProp) ? roomNameProp.GetString() ?? string.Empty : string.Empty,
                        Status = showTimeElement.TryGetProperty("status", out var statusProp) ? statusProp.GetString() ?? string.Empty : string.Empty
                    };
                    
                    showTimes.Add(showTime);
                }
            }
            
            return showTimes;
        }
    }
}

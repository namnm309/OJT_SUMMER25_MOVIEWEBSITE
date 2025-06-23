using Microsoft.AspNetCore.Mvc;
using UI.Models;
using UI.Services;
using System.Text.Json;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Diagnostics;
using System;
using System.Linq;

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

                        // Parse thành dynamic object trước để xử lý mapping
                        var moviesJson = JsonSerializer.Deserialize<JsonElement[]>(dataProp.GetRawText(), options);
                        var movies = new List<MovieViewModel>();

                        if (moviesJson != null)
                        {
                            foreach (var movieJson in moviesJson)
                            {
                                var movie = new MovieViewModel
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
                                    Images = MapImages(movieJson)
                                };

                                movies.Add(movie);
                            }
                        }

                        if (movies != null && movies.Any())
                        {
                            // Lấy tối đa 5 phim cho hero slider
                            viewModel.HeroMovies = movies.Take(5).ToList();
                            viewModel.FeaturedMovie = viewModel.HeroMovies.FirstOrDefault();

                            // Sửa logic cho FeaturedMovies - bắt đầu từ phim số 1
                            viewModel.FeaturedMovies = movies.Take(5).ToList();

                            // Đảm bảo FeaturedMovies có ít nhất 3 phim bằng cách duplicate nếu cần
                            while (viewModel.FeaturedMovies.Count < 3 && movies.Any())
                            {
                                var remainingSlots = 3 - viewModel.FeaturedMovies.Count;
                                var moviesToAdd = movies.Take(remainingSlots).ToList();
                                viewModel.FeaturedMovies.AddRange(moviesToAdd);
                            }

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


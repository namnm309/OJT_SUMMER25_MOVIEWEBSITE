using ApplicationLayer.DTO;
using ApplicationLayer.DTO.MovieManagement;
using ApplicationLayer.Services.MovieManagement;
using DomainLayer.Enum;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Cors;

namespace ControllerLayer.Controllers
{
    [ApiController]
    [Route("api/v1/movie")]
    public class MovieController : Controller
    {
        private readonly IMovieService _movieService;
        private readonly ILogger<MovieController> _logger;

        public MovieController(IMovieService movieService, ILogger<MovieController> logger)
        {
            _movieService = movieService;
            _logger = logger;
        }

        [HttpPost("Create")]
        public async Task<IActionResult> CreateMovie([FromBody] MovieCreateDto Dto)
        {
            _logger.LogInformation("Create Movie");
            return await _movieService.CreateMovie(Dto);
        }

        [HttpGet("View")]
        public async Task<IActionResult> ViewListMovie()
        {
            _logger.LogInformation("View List Movie");
            return await _movieService.ViewMovie();
        }

        [HttpGet("GetById")]
        public async Task<IActionResult> GetMovieById([FromQuery] Guid movieId)
        {
            _logger.LogInformation("Get Movie By Id: {MovieId}", movieId);
            return await _movieService.GetByIdAsync(movieId);
        }

        [HttpGet("ViewPagination")]
        public async Task<IActionResult> ViewListMovie([FromQuery] PaginationReq query)
        {
            _logger.LogInformation("View List Movie");
            return await _movieService.ViewMoviePagination(query);
        }

        [HttpPatch("Update")]
        public async Task<IActionResult> UpdateMovie([FromBody] MovieUpdateDto Dto)
        {
            _logger.LogInformation("Update Movie");
            return await _movieService.UpdateMovie(Dto);
        }

        [HttpDelete("Delete")]
        public async Task<IActionResult> DeleteMovie(Guid Id)
        {
            _logger.LogInformation("Delete Movie");
            return await _movieService.DeleteMovie(Id);
        }

        [HttpPatch("ChangeStatus")]
        public async Task<IActionResult> ChangeStatus(Guid Id, MovieStatus Status)
        {
            _logger.LogInformation("Update Movie");
            return await _movieService.ChangeStatus(Id, Status);
        }

        [HttpGet("Search")]
        public async Task<IActionResult> SearchMovie([FromQuery] string? keyword)
        {
            _logger.LogInformation("Search Movie");
            return await _movieService.SearchMovie(keyword);
        }

        [HttpGet("ViewGenre")]
        public async Task<IActionResult> ViewGenre()
        {
            _logger.LogInformation("View Genre");
            return await _movieService.GetAllGenre();
        }

        [HttpPost("CreateGenre")]
        public async Task<IActionResult> CreateGenre([FromBody] GenreCreateDto Dto)
        {
            _logger.LogInformation("Create Genre");
            return await _movieService.CreateGenre(Dto);
        }

        [HttpPatch("ChangeStatusGenre")]
        public async Task<IActionResult> ChangeStatusGenre(Guid Id)
        {
            _logger.LogInformation("Change Genre");
            return await _movieService.ChangeStatusGenre(Id);
        }

        [HttpPatch("SetFeatured")]
        public async Task<IActionResult> SetFeatured([FromQuery] Guid movieId, [FromQuery] bool isFeatured)
        {
            _logger.LogInformation("Set Featured Movie: {MovieId} to {IsFeatured}", movieId, isFeatured);
            return await _movieService.SetFeatured(movieId, isFeatured);
        }

        [HttpPatch("SetRecommended")]
        public async Task<IActionResult> SetRecommended([FromQuery] Guid movieId, [FromQuery] bool isRecommended)
        {
            _logger.LogInformation("Set Recommended Movie: {MovieId} to {IsRecommended}", movieId, isRecommended);
            return await _movieService.SetRecommended(movieId, isRecommended);
        }

        [HttpPatch("UpdateRating")]
        public async Task<IActionResult> UpdateRating([FromQuery] Guid movieId, [FromQuery] double rating)
        {
            _logger.LogInformation("Update Rating Movie: {MovieId} to {Rating}", movieId, rating);
            return await _movieService.UpdateRating(movieId, rating);
        }

        [HttpGet("GetRecommended")]
        public async Task<IActionResult> GetRecommended()
        {
            _logger.LogInformation("Get Recommended Movies");
            return await _movieService.GetRecommended();
        }

        [HttpGet("GetComingSoon")]
        public async Task<IActionResult> GetComingSoon()
        {
            _logger.LogInformation("Get Coming Soon Movies");
            return await _movieService.GetComingSoon();
        }

        [HttpGet("GetNowShowing")]
        public async Task<IActionResult> GetNowShowing()
        {
            _logger.LogInformation("Get Now Showing Movies");
            return await _movieService.GetNowShowing();
        }

    }
}

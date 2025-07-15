using ApplicationLayer.DTO;
using ApplicationLayer.DTO.MovieManagement;
using ApplicationLayer.Middlewares;
using ApplicationLayer.Services.MovieManagement;
using DomainLayer.Enum;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

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

        [Protected]
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

        [Protected]
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

        [Protected]
        [HttpPatch("Update")]
        public async Task<IActionResult> UpdateMovie([FromBody] MovieUpdateDto Dto)
        {
            _logger.LogInformation("Update Movie");
            return await _movieService.UpdateMovie(Dto);
        }

        [Protected]
        [HttpDelete("Delete")]
        public async Task<IActionResult> DeleteMovie(Guid Id)
        {
            _logger.LogInformation("Delete Movie");
            return await _movieService.DeleteMovie(Id);
        }

        [Protected]
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

        [Protected]
        [HttpGet("ViewGenre")]
        public async Task<IActionResult> ViewGenre()
        {
            _logger.LogInformation("View Genre");
            return await _movieService.GetAllGenre();
        }

        [Protected]
        [HttpPost("CreateGenre")]
        public async Task<IActionResult> CreateGenre([FromBody] GenreCreateDto Dto)
        {
            _logger.LogInformation("Create Genre");
            return await _movieService.CreateGenre(Dto);
        }

        [Protected]
        [HttpPatch("ChangeStatusGenre")]
        public async Task<IActionResult> ChangeStatusGenre(Guid Id)
        {
            _logger.LogInformation("Change Genre");
            return await _movieService.ChangeStatusGenre(Id);
        }

        [Protected]
        [HttpPatch("SetFeatured")]
        public async Task<IActionResult> SetFeatured([FromQuery] Guid movieId, [FromQuery] bool isFeatured)
        {
            _logger.LogInformation("Set Featured Movie: {MovieId} to {IsFeatured}", movieId, isFeatured);
            return await _movieService.SetFeatured(movieId, isFeatured);
        }

        [Protected]
        [HttpPatch("SetRecommended")]
        public async Task<IActionResult> SetRecommended([FromQuery] Guid movieId, [FromQuery] bool isRecommended)
        {
            _logger.LogInformation("Set Recommended Movie: {MovieId} to {IsRecommended}", movieId, isRecommended);
            return await _movieService.SetRecommended(movieId, isRecommended);
        }

        [Protected]
        [HttpPatch("UpdateRating")]
        public async Task<IActionResult> UpdateRating([FromQuery] Guid movieId, [FromQuery] double rating)
        {
            _logger.LogInformation("Update Rating Movie: {MovieId} to {Rating}", movieId, rating);
            return await _movieService.UpdateRating(movieId, rating);
        }

        [AllowAnonymous]
        [HttpGet("GetRecommended")]
        public async Task<IActionResult> GetRecommended()
        {
            _logger.LogInformation("Get Recommended Movies");
            return await _movieService.GetRecommended();
        }

        [AllowAnonymous]
        [HttpGet("GetComingSoon")]
        public async Task<IActionResult> GetComingSoon()
        {
            _logger.LogInformation("Get Coming Soon Movies");
            return await _movieService.GetComingSoon();
        }

        [AllowAnonymous]
        [HttpGet("GetNowShowing")]
        public async Task<IActionResult> GetNowShowing()
        {
            _logger.LogInformation("Get Now Showing Movies");
            return await _movieService.GetNowShowing();
        }

    }
}

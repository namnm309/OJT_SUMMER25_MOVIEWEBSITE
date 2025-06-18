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


    }
}

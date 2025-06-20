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

        [HttpGet("ViewWithPagination")]
        public async Task<IActionResult> ViewListMovies([FromQuery] PaginationReq query)
        {
            _logger.LogInformation("View List Movie");
            return await _movieService.ViewMoviesWithPagination(query);
        }

        [HttpGet("GetById")]
        public async Task<IActionResult> GetById(Guid movieId)
        {
            _logger.LogInformation("Get Movie By Id: {MovieId}", movieId);
            var movie = await _movieService.GetByIdAsync(movieId);
            if (movie == null)
                return NotFound("Movie not found");
            return Ok(new { data = movie });
        }

        [HttpGet("Search")]
        public async Task<ActionResult<List<MovieListDto>>> Search([FromQuery] string? keyword)
        {
            var movies = await _movieService.SearchAsync(keyword);

            if (!movies.Any())
            {
                // AC-03: no matches → return 404 with our message
                return NotFound(new { message = "No movies found" });
            }

            return Ok(movies);
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

        [HttpGet("genres")]
        [EnableCors("PublicAPI")]
        public async Task<IActionResult> GetAllGenres()
        {
            _logger.LogInformation("Get All Genres");
            return await _movieService.GetAllGenres();
        }

        [HttpGet("cinemarooms")]
        [EnableCors("PublicAPI")]
        public async Task<IActionResult> GetAllCinemaRooms()
        {
            _logger.LogInformation("Get All Cinema Rooms");
            return await _movieService.GetAllCinemaRooms();
        }
    }
}

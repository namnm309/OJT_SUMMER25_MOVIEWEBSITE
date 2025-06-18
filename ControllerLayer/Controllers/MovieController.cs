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
<<<<<<< HEAD
        {
            _logger.LogInformation("View List Movie");
            return await _movieService.ViewMovie();
        }

        [HttpGet("ViewPagination")]
        public async Task<IActionResult> ViewListMovie([FromQuery] PaginationReq query)
        {
            _logger.LogInformation("View List Movie");
            return await _movieService.ViewMoviePagination(query);
=======
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
>>>>>>> origin/dev
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

<<<<<<< HEAD
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


=======
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
>>>>>>> origin/dev
    }
}

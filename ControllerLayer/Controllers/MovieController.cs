using ApplicationLayer.DTO.MovieManagement;
using ApplicationLayer.Services.MovieManagement;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ControllerLayer.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class MovieController : ControllerBase
    {
        private readonly IMovieService _movieService;
        public MovieController(IMovieService movieService)
        {
            _movieService = movieService;
        }

        [HttpGet]
        public async Task<ActionResult<MovieListDto>> GetAllMovies()
        {
            var movies = await _movieService.GetAllAsync();
            return Ok(movies);
        }

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<MovieListDto>> GetById(Guid id)
        {
            var dto = await _movieService.GetByIdAsync(id);
            if (dto == null) return NotFound();
            return Ok(dto);
        }
    }
}

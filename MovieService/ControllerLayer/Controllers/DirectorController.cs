using ApplicationLayer.Services.MovieManagement;
using Microsoft.AspNetCore.Mvc;
using ApplicationLayer.DTO.MovieManagement;

namespace ControllerLayer.Controllers
{
    [ApiController]
    [Route("api/v1/director")]
    public class DirectorController : Controller
    {
        private readonly IDirectorService _directorService;
        private readonly ILogger<DirectorController> _logger;

        public DirectorController(IDirectorService directorService, ILogger<DirectorController> logger)
        {
            _directorService = directorService;
            _logger = logger;
        }

        [HttpGet("View")]
        public async Task<IActionResult> ViewDirectors()
        {
            _logger.LogInformation("View Directors");
            return await _directorService.ViewDirectors();
        }

        [HttpPost("Create")]
        public async Task<IActionResult> Create([FromBody] DirectorCreateDto dto)
        {
            _logger.LogInformation("Create Director {Name}", dto.Name);
            return await _directorService.CreateDirector(dto);
        }
    }
} 
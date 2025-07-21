using ApplicationLayer.Services.MovieManagement;
using Microsoft.AspNetCore.Mvc;
using ApplicationLayer.DTO.MovieManagement;

namespace ControllerLayer.Controllers
{
    [ApiController]
    [Route("api/v1/actor")]
    public class ActorController : Controller
    {
        private readonly IActorService _actorService;
        private readonly ILogger<ActorController> _logger;

        public ActorController(IActorService actorService, ILogger<ActorController> logger)
        {
            _actorService = actorService;
            _logger = logger;
        }

        [HttpGet("View")]
        public async Task<IActionResult> ViewActors()
        {
            _logger.LogInformation("View Actors");
            return await _actorService.ViewActors();
        }

        [HttpPost("Create")]
        public async Task<IActionResult> Create([FromBody] ActorCreateDto dto)
        {
            _logger.LogInformation("Create Actor {Name}", dto.Name);
            return await _actorService.CreateActor(dto);
        }
    }
} 
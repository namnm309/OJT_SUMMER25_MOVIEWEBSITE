using ApplicationLayer.DTO.PromotionManagement;
using ApplicationLayer.Services.UserManagement;
using Microsoft.AspNetCore.Mvc;

namespace ControllerLayer.Controllers
{
    [ApiController]
    [Route("api/v1/point")]
    public class PointController : Controller
    {
        private readonly IPointHistoryService _pointHistoryService;
        private readonly ILogger<PointController> _logger;

        public PointController(IPointHistoryService pointHistoryService, ILogger<PointController> logger)
        {
            _pointHistoryService = pointHistoryService;
            _logger = logger;
        }

        [HttpGet("view-score-history")]
        public async Task<IActionResult> ViewPointHistory(Guid UserId, [FromQuery] PointHistoryFilterDto Dto)
        {
            _logger.LogInformation("View Point-History");
            return await _pointHistoryService.ViewPointHistory(UserId, Dto);
        }
    }
}

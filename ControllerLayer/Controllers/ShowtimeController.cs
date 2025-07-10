using ApplicationLayer.DTO;
using ApplicationLayer.DTO.ShowtimeManagement;
using ApplicationLayer.Services.ShowtimeManagement;
using Microsoft.AspNetCore.Mvc;

namespace ControllerLayer.Controllers
{
    [ApiController]
    [Route("api/v1/showtime")]
    public class ShowtimeController : Controller
    {
        private readonly IShowtimeService _showtimeService;
        private readonly ILogger<ShowtimeController> _logger;

        public ShowtimeController(IShowtimeService showtimeService, ILogger<ShowtimeController> logger)
        {
            _showtimeService = showtimeService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllShowtimes()
        {
            _logger.LogInformation("Get all showtimes");
            return await _showtimeService.GetAllShowtimes();
        }

        [HttpGet("GetByDateRange")]
        public async Task<IActionResult> GetShowtimesByDateRange([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            _logger.LogInformation("Get showtimes by date range: {StartDate} to {EndDate}", startDate, endDate);
            return await _showtimeService.GetShowtimesByDateRange(startDate, endDate);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetShowtimeById(Guid id)
        {
            _logger.LogInformation("Get showtime by ID: {Id}", id);
            return await _showtimeService.GetShowtimeById(id);
        }

        [HttpPost]
        public async Task<IActionResult> CreateShowtime([FromBody] ShowtimeCreateDto dto)
        {
            _logger.LogInformation("Create showtime");
            return await _showtimeService.CreateShowtime(dto);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateShowtime(Guid id, [FromBody] ShowtimeUpdateDto dto)
        {
            _logger.LogInformation("Update showtime with ID: {Id}", id);
            dto.Id = id; // Ensure the ID matches
            return await _showtimeService.UpdateShowtime(id, dto);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteShowtime(Guid id)
        {
            _logger.LogInformation("Delete showtime with ID: {Id}", id);
            return await _showtimeService.DeleteShowtime(id);
        }

        [HttpGet("CheckConflict")]
        public async Task<IActionResult> CheckScheduleConflict(
            [FromQuery] Guid cinemaRoomId,
            [FromQuery] DateTime showDate,
            [FromQuery] TimeSpan startTime,
            [FromQuery] TimeSpan endTime,
            [FromQuery] Guid? excludeId = null)
        {
            _logger.LogInformation("Check schedule conflict for room {RoomId} on {ShowDate}", cinemaRoomId, showDate);
            return await _showtimeService.CheckScheduleConflict(cinemaRoomId, showDate, startTime, endTime, excludeId);
        }

        [HttpGet("movie/{movieId}")]
        public async Task<IActionResult> GetShowtimesByMovie(Guid movieId)
        {
            _logger.LogInformation("Get showtimes for movie: {MovieId}", movieId);
            return await _showtimeService.GetShowtimesByMovie(movieId);
        }

        [HttpGet("room/{roomId}")]
        public async Task<IActionResult> GetShowtimesByRoom(Guid roomId)
        {
            _logger.LogInformation("Get showtimes for room: {RoomId}", roomId);
            return await _showtimeService.GetShowtimesByRoom(roomId);
        }
    }
} 
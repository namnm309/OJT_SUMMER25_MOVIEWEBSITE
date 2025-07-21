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
        public async Task<IActionResult> GetAllShowtimes([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            _logger.LogInformation("Get all showtimes page {Page} size {Size}", page, pageSize);
            return await _showtimeService.GetAllShowtimes(page, pageSize);
        }

        [HttpGet("GetByMonth")]
        public async Task<IActionResult> GetShowtimesByMonth([FromQuery] int month, [FromQuery] int year)
        {
            _logger.LogInformation("Get showtimes by month: {Month}/{Year}", month, year);
            return await _showtimeService.GetShowtimesByMonth(month, year);
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

        [HttpPost("create-new")]
        public async Task<IActionResult> CreateNewShowtime([FromBody] ShowtimeCreateNewDto dto)
        {
            _logger.LogInformation("Create new showtime");
            return await _showtimeService.CreateNewShowtime(dto);
        }

        [HttpGet("movies-dropdown")]
        public async Task<IActionResult> GetMoviesForDropdown()
        {
            _logger.LogInformation("Get movies for dropdown");
            return await _showtimeService.GetMoviesForDropdown();
        }

        [HttpGet("cinema-rooms-dropdown")]
        public async Task<IActionResult> GetCinemaRoomsForDropdown()
        {
            _logger.LogInformation("Get cinema rooms for dropdown");
            return await _showtimeService.GetCinemaRoomsForDropdown();
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

        [HttpGet("Debug")]
        public async Task<IActionResult> DebugShowtimes()
        {
            try
            {
                _logger.LogInformation("Debug: Getting all showtimes");
                var allResult = await _showtimeService.GetAllShowtimes();
                
                _logger.LogInformation("Debug: Getting showtimes by month");
                var monthResult = await _showtimeService.GetShowtimesByMonth(1, 2025);
                
                return Json(new { 
                    success = true,
                    allShowtimes = allResult,
                    monthShowtimes = monthResult,
                    message = "Debug API worked successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in debug API");
                return Json(new { 
                    success = false, 
                    error = ex.Message,
                    stackTrace = ex.ToString()
                });
            }
        }
    }
}
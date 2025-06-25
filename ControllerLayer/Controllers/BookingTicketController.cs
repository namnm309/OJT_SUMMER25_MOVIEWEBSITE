using ApplicationLayer.Services.BookingTicketManagement;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ControllerLayer.Controllers
{
    [ApiController]
    [Route("api/v1/booking-ticket")]
    public class BookingTicketController : Controller
    {
        private readonly IBookingTicketService _bookingTicketService;
        private readonly ILogger<BookingTicketController> _logger;

        public BookingTicketController(IBookingTicketService bookingTicketService, ILogger<BookingTicketController> logger)
        {
            _bookingTicketService = bookingTicketService;
            _logger = logger;
        }

        [HttpGet("dropdown/movies")]
        public async Task<IActionResult> GetAvailableMovies()
        {
            _logger.LogInformation("Get Available Movies");
            return await _bookingTicketService.GetAvailableMovies();
        }

        [HttpGet("dropdown/movies/{movieId}/dates")]
        public async Task<IActionResult> GetShowDates(Guid movieId) //Lấy danh sách ngày chiếu cho phim
        {
            _logger.LogInformation("Get ShowDates");
            return await _bookingTicketService.GetShowDatesByMovie(movieId);
        } 

        [HttpGet("dropdown/movies/{movieId}/times")]
        public async Task<IActionResult> GetShowTimes(Guid movieId, [FromQuery] DateTime date) //Lấy các giờ chiếu
        {
            _logger.LogInformation("Get ShowTimes");
            return await _bookingTicketService.GetShowTimesByMovieAndDate(movieId, date);
        }

        [HttpGet("{bookingId}/details")]
        [Authorize(Roles = "Member")]
        public async Task<IActionResult> GetBookingDetails(Guid bookingId)
        {
            // Get user ID from claims
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out Guid userId))
            {
                return Unauthorized("User not authenticated");
            }

            return await _bookingTicketService.GetBookingDetailsAsync(bookingId, userId);
        }

    }
}

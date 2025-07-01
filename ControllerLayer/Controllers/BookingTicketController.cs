<<<<<<< HEAD
﻿using ApplicationLayer.DTO.BookingTicketManagement;
using ApplicationLayer.Services.BookingTicketManagement;
=======
﻿using ApplicationLayer.Services.BookingTicketManagement;
using Microsoft.AspNetCore.Authorization;
>>>>>>> 6ba535ce8d8b07b079ee983895159e03eaa44924
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ControllerLayer.Controllers
{
    [ApiController]
    [Route("api/v1/booking-ticket")]
    public class BookingTicketController : Controller
    {
        private readonly IBookingTicketService _bookingTicketService;
        private readonly ISeatService _seatService;
        private readonly ILogger<BookingTicketController> _logger;

        public BookingTicketController(IBookingTicketService bookingTicketService, ISeatService seatService, ILogger<BookingTicketController> logger)
        {
            _bookingTicketService = bookingTicketService;
            _seatService = seatService;
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

<<<<<<< HEAD
        [HttpGet("available")]
        public async Task<IActionResult> GetAvailableSeats([FromQuery] Guid showTimeId)
        {
            _logger.LogInformation("Get available seats for showtime: {ShowTimeId}", showTimeId);
            return await _seatService.GetAvailableSeats(showTimeId);
        }

        [HttpPost("validate")]
        public async Task<IActionResult> ValidateSelectedSeats(
            [FromQuery] Guid showTimeId,
            [FromBody] List<Guid> seatIds)
        {
            _logger.LogInformation("Validating selected seats: {SeatIds} for showtime: {ShowTimeId}",
                string.Join(",", seatIds), showTimeId);

            return await _seatService.ValidateSelectedSeats(showTimeId, seatIds);
        }

        [HttpGet("{showTimeId}/details")] // New endpoint
        public async Task<IActionResult> GetShowTimeDetails(Guid showTimeId)
        {
            _logger.LogInformation("Getting details for showtime: {ShowTimeId}", showTimeId);
            return await _seatService.GetShowTimeDetails(showTimeId);
        }

        [HttpPost("confirm")]
        public async Task<IActionResult> ConfirmBooking([FromBody] ConfirmBookingRequestDto request)
        {
            _logger.LogInformation("Confirming booking for ShowtimeId: {ShowTimeId}, UserId: {UserId}",
                request.ShowtimeId, request.UserId); // Log UserID nếu có

            return await _bookingTicketService.ConfirmBooking(request);
        }
=======
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

>>>>>>> 6ba535ce8d8b07b079ee983895159e03eaa44924
    }
}

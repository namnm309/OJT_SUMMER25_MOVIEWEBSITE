using ApplicationLayer.DTO.BookingTicketManagement;
﻿using ApplicationLayer.Services.BookingTicketManagement;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using InfrastructureLayer.Data;

namespace ControllerLayer.Controllers
{
    [ApiController]
    [Route("api/v1/booking-ticket")]
    public class BookingTicketController : Controller
    {
        private readonly IBookingTicketService _bookingTicketService;
        private readonly ISeatService _seatService;
        private readonly ILogger<BookingTicketController> _logger;
        private readonly MovieContext _context;

        public BookingTicketController(IBookingTicketService bookingTicketService, ISeatService seatService, ILogger<BookingTicketController> logger, MovieContext context)
        {
            _bookingTicketService = bookingTicketService;
            _seatService = seatService;
            _logger = logger;
            _context = context;
        }

        [HttpGet("test-seed-data")]
        public async Task<IActionResult> TestSeedData()
        {
            try
            {
                await DataSeeder.SeedAdminUser(_context);
                await DataSeeder.SeedSampleData(_context);
                return Ok(new { message = "✅ Dữ liệu mẫu đã được tạo thành công!" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"❌ Lỗi: {ex.Message}" });
            }
        }

        [HttpGet("dropdown/movies")]
        public async Task<IActionResult> GetAvailableMovies()
        {
            _logger.LogInformation("Get Available Movies");
            return await _bookingTicketService.GetAvailableMovies();
        }

        [HttpGet("dropdown/movies/{movieId}/dates")]
        public async Task<IActionResult> GetShowDates(Guid movieId) // Lấy danh sách ngày chiếu theo phim
        {
            _logger.LogInformation("Get ShowDates");
            return await _bookingTicketService.GetShowDatesByMovie(movieId);
        } 

        [HttpGet("dropdown/movies/{movieId}/times")]
        public async Task<IActionResult> GetShowTimes(Guid movieId, [FromQuery] DateTime date) // Lấy giờ chiếu theo phim và ngày
        {
            _logger.LogInformation("Get ShowTimes");
            return await _bookingTicketService.GetShowTimesByMovieAndDate(movieId, date);
        }

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

        [HttpGet("showtime/{showTimeId}/details")] // Endpoint lấy chi tiết suất chiếu
        public async Task<IActionResult> GetShowTimeDetails(Guid showTimeId)
        {
            _logger.LogInformation("Getting details for showtime: {ShowTimeId}", showTimeId);
            return await _seatService.GetShowTimeDetails(showTimeId);
        }

        [HttpPost("confirm-user-booking")]
        public async Task<IActionResult> ConfirmBooking([FromBody] ConfirmBookingRequestDto request)
        {
            _logger.LogInformation("Confirming booking for ShowtimeId: {ShowTimeId}, UserId: {UserId}",
                request.ShowtimeId, request.UserId); // Ghi log thông tin người dùng

            return await _bookingTicketService.ConfirmUserBooking(request);
        }

        [HttpPost("check-member")]
        public async Task<IActionResult> CheckMember([FromBody] CheckMemberRequestDto request)
        {
            _logger.LogInformation("Checking member with request: {@Request}", request);
            return await _bookingTicketService.CheckMember(request);
        }

        [HttpPost("create-member-account")]
        public async Task<IActionResult> CreateMemberAccount([FromBody] CreateMemberAccountDto request)
        {
            _logger.LogInformation("Creating member account with request: {@Request}", request);
            var result = await _bookingTicketService.CreateMemberAccount(request);

            if (result.Success)
            {
                return Ok(new { Message = result.message });
            }
            else
            {
                return BadRequest(new { Message = result.message });
            }
        }

        [HttpPost("confirm-Admin-booking")]
        public async Task<IActionResult> ConfirmAdminBooking([FromBody] ConfirmBookingRequestAdminDto request)
        {
            _logger.LogInformation("Confirming booking with request: {@Request}", request);
            return await _bookingTicketService.ConfirmAdminBooking(request);
        }

        [HttpGet("booking/{bookingCode}")]
        public async Task<IActionResult> GetBookingDetails(string bookingCode)
        {
            _logger.LogInformation("Getting booking details for code: {BookingCode}", bookingCode);
            return await _bookingTicketService.GetBookingDetails(bookingCode);
        }

        [HttpGet("member-booking/{bookingId}/details")]
        [Authorize(Roles = "Member")]
        public async Task<IActionResult> GetMemberBookingDetails(Guid bookingId)
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

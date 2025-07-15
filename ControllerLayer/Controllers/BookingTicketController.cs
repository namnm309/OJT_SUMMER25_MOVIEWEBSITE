using ApplicationLayer.DTO.BookingTicketManagement;
﻿using ApplicationLayer.Services.BookingTicketManagement;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using InfrastructureLayer.Data;
using ApplicationLayer.Services.UserManagement;
using ApplicationLayer.DTO.UserManagement;
using ApplicationLayer.Middlewares;

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
        private readonly ICustomerSearchService _customerSearchService;
        private readonly IUserService _userService;

        public BookingTicketController(IBookingTicketService bookingTicketService, ISeatService seatService, ILogger<BookingTicketController> logger, MovieContext context, ICustomerSearchService customerSearchService, IUserService userService)
        {
            _bookingTicketService = bookingTicketService;
            _seatService = seatService;
            _logger = logger;
            _context = context;
            _customerSearchService = customerSearchService;
            _userService = userService;
        }

        [HttpGet("dropdown/movies")]
        [AllowAnonymous]
        public async Task<IActionResult> GetAvailableMovies()
        {
            return await _bookingTicketService.GetAvailableMovies();
        }

        [HttpGet("dropdown/movies/{movieId}/dates")]
        public async Task<IActionResult> GetShowDates(Guid movieId) // Lấy danh sách ngày chiếu theo phim
        {
            return await _bookingTicketService.GetShowDatesByMovie(movieId);
        } 

        [HttpGet("dropdown/movies/{movieId}/times")]
        public async Task<IActionResult> GetShowTimes(Guid movieId, [FromQuery] DateTime date) // Lấy giờ chiếu theo phim và ngày
        {
            return await _bookingTicketService.GetShowTimesByMovieAndDate(movieId, date);
        }

        [Protected]
        [HttpGet("available")]
        public async Task<IActionResult> GetAvailableSeats([FromQuery] Guid showTimeId)
        {
            return await _seatService.GetAvailableSeats(showTimeId);
        }

        [Protected]
        [HttpPost("validate")]
        public async Task<IActionResult> ValidateSelectedSeats(
            [FromQuery] Guid showTimeId,
            [FromBody] List<Guid> seatIds)
        {
            return await _seatService.ValidateSelectedSeats(showTimeId, seatIds);
        }

        [HttpGet("showtime/{showTimeId}/details")] // Endpoint lấy chi tiết suất chiếu
        public async Task<IActionResult> GetShowTimeDetails(Guid showTimeId)
        {
            return await _seatService.GetShowTimeDetails(showTimeId);
        }

        [HttpPost("confirm-user-booking")]
        public async Task<IActionResult> ConfirmBooking([FromBody] ConfirmBookingRequestDto request)
        {
            return await _bookingTicketService.ConfirmUserBooking(request);
        }

        [HttpPost("confirm-user-booking-v2")]
        public async Task<IActionResult> ConfirmBookingV2([FromBody] ConfirmBookingRequestDto request)
        {
            return await _bookingTicketService.ConfirmUserBookingV2(request);
        }

        [HttpPost("check-member")]
        public async Task<IActionResult> CheckMember([FromBody] CheckMemberRequestDto request)
        {
            return await _bookingTicketService.CheckMember(request);
        }

        [HttpPost("create-member-account")]
        public async Task<IActionResult> CreateMemberAccount([FromBody] CreateMemberAccountDto request)
        {
            var result = await _bookingTicketService.CreateMemberAccount(request);

            if (result.Success)
            {
                return Ok(new { success = true, message = result.message });
            }
            else
            {
                return BadRequest(new { success = false, message = result.message });
            }
        }

        [HttpPost("confirm-Admin-booking")]
        public async Task<IActionResult> ConfirmAdminBooking([FromBody] ConfirmBookingRequestAdminDto request)
        {
            return await _bookingTicketService.ConfirmAdminBooking(request);
        }

        [HttpGet("booking/{bookingCode}")]
        public async Task<IActionResult> GetBookingDetails(string bookingCode)
        {
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

        /// <summary>
        /// Tìm kiếm khách hàng theo email hoặc số điện thoại
        /// </summary>
        /// <param name="searchTerm">Email hoặc số điện thoại để tìm kiếm</param>
        /// <returns>Thông tin chi tiết của khách hàng</returns>
        [HttpGet("SearchCustomer")]
        public async Task<IActionResult> SearchCustomer([FromQuery] string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                return BadRequest(new { message = "Vui lòng nhập số điện thoại hoặc email" });
            }

            try 
            {
                var customer = await _userService.SearchCustomerAsync(searchTerm);

                if (customer == null)
                {
                    return NotFound(new { message = "Không tìm thấy khách hàng" });
                }

                return Ok(customer);
            }
            catch (Exception ex)
            {
                // Log lỗi nếu cần
                return StatusCode(500, new { message = "Có lỗi xảy ra khi tìm kiếm khách hàng" });
            }
        }

        /// <summary>
        /// Tìm kiếm khách hàng theo số điện thoại
        /// </summary>
        /// <param name="phoneNumber">Số điện thoại để tìm kiếm</param>
        /// <returns>Thông tin chi tiết của khách hàng</returns>
        [HttpGet("SearchCustomerByPhone")]
        public async Task<IActionResult> SearchCustomerByPhone(string phoneNumber)
        {
            try
            {
                var customer = await _customerSearchService.SearchCustomerByPhoneAsync(phoneNumber);

                if (customer == null)
                {
                    return NotFound(new { message = "Không tìm thấy khách hàng với số điện thoại này" });
                }

                return Json(customer);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Tìm kiếm khách hàng theo email
        /// </summary>
        /// <param name="email">Email để tìm kiếm</param>
        /// <returns>Thông tin chi tiết của khách hàng</returns>
        [HttpGet("SearchCustomerByEmail")]
        public async Task<IActionResult> SearchCustomerByEmail(string email)
        {
            try
            {
                var customer = await _customerSearchService.SearchCustomerByEmailAsync(email);

                if (customer == null)
                {
                    return NotFound(new { message = "Không tìm thấy khách hàng với email này" });
                }

                return Json(customer);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Get booking confirmation details for display (AC-01)
        /// </summary>
        [HttpGet("booking-confirmation-detail")]
        public async Task<IActionResult> GetBookingConfirmationDetail(
            [FromQuery] Guid showTimeId, 
            [FromQuery] List<Guid> seatIds, 
            [FromQuery] string memberId)
        {
            return await _bookingTicketService.GetBookingConfirmationDetailAsync(showTimeId, seatIds, memberId);
        }

        /// <summary>
        /// Confirm booking with score conversion options (AC-02, AC-03, AC-04, AC-05)
        /// </summary>
        [HttpPost("confirm-booking-with-score")]
        public async Task<IActionResult> ConfirmBookingWithScore([FromBody] BookingConfirmWithScoreRequestDto request)
        {
            return await _bookingTicketService.ConfirmBookingWithScoreAsync(request);
        }

        /// <summary>
        /// Get list of all bookings with filtering and pagination for admin/staff
        /// </summary>
        [HttpGet("bookings")]
        //[Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> GetBookingList([FromQuery] BookingFilterDto filter)
        {
            return await _bookingTicketService.GetBookingListAsync(filter);
        }

        /// <summary>
        /// Update booking status
        /// </summary>
        [HttpPut("booking/{bookingId}/status")]
        //[Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> UpdateBookingStatus(Guid bookingId, [FromBody] UpdateBookingStatusDto request)
        {
            return await _bookingTicketService.UpdateBookingStatusAsync(bookingId, request.NewStatus);
        }

        /// <summary>
        /// Cancel a booking
        /// </summary>
        [HttpPost("booking/{bookingId}/cancel")]
        //[Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> CancelBooking(Guid bookingId, [FromBody] CancelBookingDto request)
        {
            return await _bookingTicketService.CancelBookingAsync(bookingId, request.Reason);
        }

        [Protected]
        [HttpPost("confirm")]
        public async Task<IActionResult> ConfirmBooking([FromBody] ConfirmBookingRequest req)
        {
            return await _seatService.ConfirmBookingAsync(req);
        }

        [Protected]
        [HttpPost("booking/cancel-booking")]
        public async Task<IActionResult> Cancel(Guid bookingId)
        {
            return await _seatService.CancelBooking(bookingId);
        }
    }
}

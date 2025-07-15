using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using UI.Areas.BookingManagement.Models;
using UI.Areas.BookingManagement.Services;

namespace UI.Areas.BookingManagement.Controllers
{
    [Area("BookingManagement")]
    [Authorize(Roles = "Admin,Staff")]
    public class BookingTicketController : Controller
    {
        private readonly IBookingManagementUIService _bookingService;
        private readonly ILogger<BookingTicketController> _logger;

        public BookingTicketController(IBookingManagementUIService bookingService, ILogger<BookingTicketController> logger)
        {
            _bookingService = bookingService;
            _logger = logger;
        }

        // Trang chính của tính năng đặt vé trong dashboard
        public IActionResult Index()
        {
            ViewBag.CurrentPage = "BookingManagement";
            return View();
        }

        // Trang danh sách đặt vé
        public IActionResult BookingList()
        {
            ViewBag.CurrentPage = "BookingList";
            return View();
        }

        // API endpoints cho tính năng đặt vé
        [HttpGet]
        public async Task<IActionResult> GetMovies()
        {
            try
            {
                var response = await _bookingService.GetMoviesAsync();
                return Json(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting movies for booking");
                return Json(new { success = false, message = "Có lỗi xảy ra khi tải danh sách phim" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetShowTimes(Guid movieId, DateTime showDate)
        {
            try
            {
                var response = await _bookingService.GetShowTimesAsync(movieId, showDate);
                return Json(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting showtimes for movie {MovieId} on {ShowDate}", movieId, showDate);
                return Json(new { success = false, message = "Có lỗi xảy ra khi tải lịch chiếu" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetShowDates(Guid movieId)
        {
            try
            {
                var response = await _bookingService.GetShowDatesAsync(movieId);
                return Json(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting show dates for movie {MovieId}", movieId);
                return Json(new { success = false, message = "Có lỗi xảy ra khi tải ngày chiếu" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetSeats(Guid showTimeId)
        {
            try
            {
                var response = await _bookingService.GetSeatsAsync(showTimeId);
                return Json(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting seats for showtime {ShowTimeId}", showTimeId);
                return Json(new { success = false, message = "Có lỗi xảy ra khi tải sơ đồ ghế" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> SearchCustomer(string phoneNumber)
        {
            try
            {
                // Gọi API để tìm kiếm khách hàng theo số điện thoại
                // Tạm thời trả về dữ liệu mẫu
                var customer = new
                {
                    success = true,
                    data = new
                    {
                        id = Guid.NewGuid(),
                        fullName = "Nguyễn Văn A",
                        phoneNumber = phoneNumber,
                        email = "nguyenvana@email.com",
                        points = 150
                    }
                };
                return Json(customer);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching customer with phone {PhoneNumber}", phoneNumber);
                return Json(new { success = false, message = "Có lỗi xảy ra khi tìm kiếm khách hàng" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateBooking([FromBody] BookingConfirmViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return Json(new { success = false, message = "Dữ liệu không hợp lệ" });
                }

                // Lấy thông tin user hiện tại (staff/admin)
                var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(currentUserId))
                {
                    return Json(new { success = false, message = "Không thể xác định người dùng" });
                }

                // Gọi service để tạo booking và cập nhật trạng thái ghế thành "Pending"
                var response = await _bookingService.ConfirmBookingAsync(model);

                if (response.Success)
                {
                    return Json(new
                    {
                        success = true,
                        message = "Tạo booking thành công",
                        bookingId = response.Data // Trả về BookingId để dùng cho payment
                    });
                }
                else
                {
                    return Json(new { success = false, message = response.Message });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating booking");
                return Json(new { success = false, message = "Có lỗi xảy ra khi đặt vé" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> ProcessVNPayPayment([FromBody] dynamic paymentData)
        {
            try
            {
                // Xử lý thanh toán VNPay
                // Tạm thời trả về success
                return Json(new
                {
                    success = true,
                    message = "Thanh toán VNPay thành công",
                    paymentUrl = "https://sandbox.vnpayment.vn/paymentv2/vpcpay.html"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing VNPay payment");
                return Json(new { success = false, message = "Có lỗi xảy ra khi xử lý thanh toán" });
            }
        }

        // API endpoints cho danh sách đặt vé
        [HttpGet]
        public async Task<IActionResult> GetBookingList(
            DateTime? fromDate = null,
            DateTime? toDate = null,
            string? movieTitle = null,
            string? bookingStatus = null,
            string? customerSearch = null,
            string? bookingCode = null,
            int page = 1,
            int pageSize = 10,
            string sortBy = "BookingDate",
            string sortDirection = "desc")
        {
            try
            {
                var response = await _bookingService.GetBookingListAsync(new
                {
                    FromDate = fromDate,
                    ToDate = toDate,
                    MovieTitle = movieTitle,
                    BookingStatus = bookingStatus,
                    CustomerSearch = customerSearch,
                    BookingCode = bookingCode,
                    Page = page,
                    PageSize = pageSize,
                    SortBy = sortBy,
                    SortDirection = sortDirection
                });

                return Json(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting booking list");
                return Json(new { success = false, message = "Có lỗi xảy ra khi tải danh sách đặt vé" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetBookingDetail(Guid bookingId)
        {
            try
            {
                var response = await _bookingService.GetBookingDetailAsync(bookingId);
                return Json(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting booking detail for {BookingId}", bookingId);
                return Json(new { success = false, message = "Có lỗi xảy ra khi tải chi tiết đặt vé" });
            }
        }

        [HttpPut]
        public async Task<IActionResult> UpdateBookingStatus(Guid bookingId, [FromBody] dynamic statusData)
        {
            try
            {
                string newStatus = statusData.newStatus;
                var response = await _bookingService.UpdateBookingStatusAsync(bookingId, newStatus);

                if (response.Success)
                {
                    return Json(new { success = true, message = "Cập nhật trạng thái thành công" });
                }
                else
                {
                    return Json(new { success = false, message = response.Message });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating booking status for {BookingId}", bookingId);
                return Json(new { success = false, message = "Có lỗi xảy ra khi cập nhật trạng thái" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> CancelBooking(Guid bookingId, [FromBody] dynamic cancelData)
        {
            try
            {
                string reason = cancelData.reason;
                var response = await _bookingService.CancelBookingAsync(bookingId, reason);

                if (response.Success)
                {
                    return Json(new { success = true, message = "Hủy đặt vé thành công" });
                }
                else
                {
                    return Json(new { success = false, message = response.Message });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling booking {BookingId}", bookingId);
                return Json(new { success = false, message = "Có lỗi xảy ra khi hủy đặt vé" });
            }
        }
    }
}
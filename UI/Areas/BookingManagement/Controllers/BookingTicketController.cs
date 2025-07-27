using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using UI.Areas.BookingManagement.Models;
using UI.Areas.BookingManagement.Services;
using UI.Services;

namespace UI.Areas.BookingManagement.Controllers
{
    [Area("BookingManagement")]
    [Authorize(Roles = "Admin,Staff")]
    public class BookingTicketController : Controller
    {
        private readonly IBookingManagementUIService _bookingService;
        private readonly ILogger<BookingTicketController> _logger;
        private readonly IApiService _apiService;

        public BookingTicketController(IBookingManagementUIService bookingService, ILogger<BookingTicketController> logger, IApiService apiService)
        {
            _bookingService = bookingService;
            _logger = logger;
            _apiService = apiService;
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

        // === New VNPay Integration ===

        [HttpPost]
        public async Task<IActionResult> CreateVnpayPayment([FromBody] CreateVnpayRequest request)
        {
            if (request == null || request.BookingId == Guid.Empty)
            {
                return Json(new { success = false, message = "Dữ liệu không hợp lệ" });
            }

            try
            {
                // Tạo returnUrl để VNPay callback về
                var resp = await _bookingService.CreateVnpayPaymentAsync(
                    request.BookingId,
                    request.Amount,
                    request.Decription ?? "Thanh toan VNPay");
                
                if (resp.Success && !string.IsNullOrEmpty(resp.Data))
                {
                    // Backend trả về URL dạng string thuần túy
                    return Json(new { success = true, paymentUrl = resp.Data });
                }
                
                return Json(new { success = false, message = resp.Message ?? "Không thể tạo link thanh toán" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating VNPay payment for booking {BookingId}", request.BookingId);
                return Json(new { success = false, message = "Có lỗi xảy ra khi khởi tạo thanh toán VNPay" });
            }
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> PaymentSuccess(string bookingCode, string bookingId = null)
        {
            object bookingInfo = null;
            if (!string.IsNullOrEmpty(bookingId))
            {
                // Ưu tiên lấy theo bookingId cho admin
                var beResp = await _apiService.GetAsync<System.Text.Json.JsonElement>($"api/v1/booking-ticket/admin-booking/{bookingId}");
                if (beResp.Success && beResp.Data.ValueKind == System.Text.Json.JsonValueKind.Object)
                {
                    bookingInfo = beResp.Data;
                }
            }
            if (bookingInfo == null && !string.IsNullOrEmpty(bookingCode))
            {
                // Fallback lấy theo bookingCode như cũ
                var bookingResp = await _bookingService.GetBookingByCodeAsync(bookingCode);
                bookingInfo = bookingResp.Data;
            }
            ViewBag.Booking = bookingInfo;
            return View("~/Areas/BookingManagement/Views/BookingTicket/PaymentSuccess.cshtml");
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> PaymentFail(string bookingCode, string bookingId = null)
        {
            object bookingInfo = null;
            if (!string.IsNullOrEmpty(bookingId))
            {
                var beResp = await _apiService.GetAsync<System.Text.Json.JsonElement>($"api/v1/booking-ticket/admin-booking/{bookingId}");
                if (beResp.Success && beResp.Data.ValueKind == System.Text.Json.JsonValueKind.Object)
                {
                    bookingInfo = beResp.Data;
                }
            }
            if (bookingInfo == null && !string.IsNullOrEmpty(bookingCode))
            {
                var bookingResp = await _bookingService.GetBookingByCodeAsync(bookingCode);
                bookingInfo = bookingResp.Data;
            }
            ViewBag.Booking = bookingInfo;
            return View("~/Areas/BookingManagement/Views/BookingTicket/PaymentFail.cshtml");
        }

        // Xử lý callback từ VNPay
        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> VnpayReturn(
            string vnp_ResponseCode,
            string vnp_TransactionStatus, 
            string vnp_TxnRef,
            string vnp_Amount,
            string vnp_OrderInfo)
        {
            try
            {
                _logger.LogInformation("VNPay return - ResponseCode: {ResponseCode}, Status: {Status}, TxnRef: {TxnRef}", 
                    vnp_ResponseCode, vnp_TransactionStatus, vnp_TxnRef);

                // vnp_TxnRef thường chứa bookingCode hoặc bookingId
                string bookingCode = vnp_TxnRef;

                // Kiểm tra kết quả thanh toán
                // ResponseCode = 00: Thành công
                // ResponseCode = 24: Hủy giao dịch
                // TransactionStatus = 00: Thành công, 02: Thất bại
                
                if (vnp_ResponseCode == "00" && vnp_TransactionStatus == "00")
                {
                    // Thanh toán thành công
                    return RedirectToAction("PaymentSuccess", new { bookingCode = bookingCode });
                }
                else
                {
                    // Thanh toán thất bại hoặc hủy
                    return RedirectToAction("PaymentFail", new { bookingCode = bookingCode });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing VNPay return");
                return RedirectToAction("PaymentFail", new { bookingCode = vnp_TxnRef });
            }
        }

        public class CreateVnpayRequest
        {
            public Guid BookingId { get; set; }
            public decimal Amount { get; set; }
            public string? Decription { get; set; }
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
        public async Task<IActionResult> UpdateBookingStatusApi(Guid bookingId, [FromBody] dynamic statusData)
        {
            try
            {
                string newStatus = statusData.newStatus;
                var response = await _bookingService.UpdateBookingStatusAsync(bookingId, newStatus);

                if (response.Success)
                {
                    return Json(new { success = true, message = "Cập nhật trạng thái thành công", data = response.Data });
                }
                else
                {
                    return Json(new { success = false, message = response.Message, data = (object)null });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating booking status for {BookingId}", bookingId);
                return Json(new { success = false, message = "Có lỗi xảy ra khi cập nhật trạng thái", data = (object)null });
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

        [HttpGet]
        public async Task<IActionResult> GetBookingIdByCode(string bookingCode)
        {
            if (string.IsNullOrEmpty(bookingCode)) return Json(new { success = false, message = "bookingCode required" });

            var beResp = await _apiService.GetAsync<System.Text.Json.JsonElement>("api/v1/booking-ticket/booking-id/" + bookingCode);

            if (beResp.Success && beResp.Data.ValueKind == System.Text.Json.JsonValueKind.Object)
            {
                if (beResp.Data.TryGetProperty("bookingId", out var idProp))
                {
                    return Json(new { bookingId = idProp.GetString() });
                }
            }

            return StatusCode(500, new { message = beResp.Message ?? "Không tìm thấy bookingId" });
        }

        [HttpGet]
        public async Task<IActionResult> GetPromotions()
        {
            try
            {
                var response = await _bookingService.GetPromotionsAsync();
                if (response.Success)
                    return Json(new { success = true, data = response.Data, message = "Lấy danh sách khuyến mãi thành công" });
                return Json(new { success = false, data = new object[0], message = response.Message ?? "Không có khuyến mãi" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting promotions for booking");
                return Json(new { success = false, data = new object[0], message = "Có lỗi xảy ra khi tải danh sách khuyến mãi" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> ConfirmBookingWithScore([FromBody] BookingConfirmWithScoreViewModel model)
        {
            // Gọi API backend thực sự qua _apiService hoặc _bookingService
            var beResp = await _apiService.PostAsync<object>(
                "api/v1/booking-ticket/confirm-booking-with-score", model);

            if (beResp.Success)
                return Json(new { success = true, message = "Booking confirmed successfully!", data = beResp.Data });
            return StatusCode(500, new { success = false, message = beResp.Message ?? "Có lỗi xảy ra", data = (object)null });
        }

    }
}
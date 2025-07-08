using Microsoft.AspNetCore.Mvc;
using UI.Areas.BookingManagement.Services; // Service quản lý đặt vé
using UI.Areas.BookingManagement.Models; // Models cho BookingManagement
using UI.Models; // Model dữ liệu UI
using System.Linq; // Hỗ trợ LINQ

namespace UI.Controllers
{
    public class BookTicketController : Controller
    {
        private readonly IBookingManagementUIService _bookingService;
        private readonly ILogger<BookTicketController> _logger; // Inject logger

        public BookTicketController(IBookingManagementUIService bookingService, ILogger<BookTicketController> logger)
        {
            _bookingService = bookingService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> SelectMovieAndShowtime()
        {
            ViewBag.Title = "Đặt Vé Nhanh";
            var model = new BookingDropdownViewModel();

            try
            {
                // 1. Get Movies
                var moviesResult = await _bookingService.GetMoviesDropdownAsync();
                if (moviesResult.Success && moviesResult.Data != null)
                {
                    model.Movies = moviesResult.Data;
                    _logger.LogInformation("Successfully loaded {Count} movies.", model.Movies.Count);
                }
                else
                {
                    TempData["ErrorMessage"] = moviesResult.Message ?? "Không thể tải danh sách phim.";
                    _logger.LogError("Failed to load movies: {Message}", moviesResult.Message);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in SelectMovieAndShowtime while loading movies.");
                TempData["ErrorMessage"] = "Đã xảy ra lỗi khi tải danh sách phim.";
            }

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> GetShowDates(Guid movieId)
        {
            try
            {
                var datesResult = await _bookingService.GetShowDatesAsync(movieId);
                if (datesResult.Success && datesResult.Data != null)
                {
                    // API trả về dạng { Code: "yyyy-MM-dd", Text: "dd/MM" }
                    // Trả về nguyên dạng cho client xử lý
                    return Json(new { success = true, dates = datesResult.Data });
                }
                return Json(new { success = false, message = datesResult.Message ?? "Không thể tải ngày chiếu." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetShowDates for movie {MovieId}", movieId);
                return Json(new { success = false, message = "Đã xảy ra lỗi khi tải ngày chiếu." });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetShowTimes(Guid movieId, DateTime showDate)
        {
            try
            {
                var timesResult = await _bookingService.GetShowTimesAsync(movieId, showDate);
                if (timesResult.Success && timesResult.Data != null)
                {
                    return Json(new { success = true, times = timesResult.Data });
                }
                return Json(new { success = false, message = timesResult.Message ?? "Không thể tải suất chiếu." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetShowTimes for movie {MovieId} on {ShowDate}", movieId, showDate);
                return Json(new { success = false, message = "Đã xảy ra lỗi khi tải suất chiếu." });
            }
        }

        // select seat ---------------------------------------------------------------------------------------
        [HttpGet]
        public IActionResult SelectSeat(Guid showtimeId)
        {
            return RedirectToAction("Index", "SelectSeat", new { showtimeId });
        }

        /// <summary>
        /// Tìm kiếm khách hàng theo số điện thoại hoặc email
        /// </summary>
        /// <param name="searchTerm">Số điện thoại hoặc email</param>
        /// <returns>Thông tin khách hàng</returns>
        [HttpGet]
        public async Task<IActionResult> SearchCustomer(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                return Json(new { success = false, message = "Vui lòng nhập số điện thoại hoặc email" });
            }

            try
            {
                var result = await _bookingService.SearchCustomerAsync(searchTerm);
                
                if (result.Success && result.Data != null)
                {
                    // Chuyển đổi DateTime? để tránh lỗi serialization
                    var customerData = new
                    {
                        id = result.Data.Id,
                        fullName = result.Data.FullName,
                        email = result.Data.Email,
                        phoneNumber = result.Data.PhoneNumber,
                        points = result.Data.Points,
                        totalBookings = result.Data.TotalBookings,
                        lastBookingDate = result.Data.LastBookingDate?.ToString("yyyy-MM-dd") ?? null
                    };
                    
                    return Json(new { success = true, data = customerData });
                }
                else
                {
                    return Json(new { success = false, message = result.Message ?? "Không tìm thấy khách hàng" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in SearchCustomer for term {SearchTerm}", searchTerm);
                return Json(new { success = false, message = "Có lỗi xảy ra khi tìm kiếm khách hàng" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateCustomer([FromBody] CreateCustomerViewModel model)
        {
            try
            {
                var result = await _bookingService.CreateCustomerAsync(model);
                if (result.Success)
                {
                    return Json(new { success = true, message = "Tạo khách hàng thành công" });
                }
                else
                {
                    return Json(new { success = false, message = result.Message ?? "Không thể tạo khách hàng" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Có lỗi xảy ra khi tạo khách hàng" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> ConfirmAdminBooking([FromBody] ConfirmAdminBookingViewModel model)
        {
            try
            {
                // Validation
                if (model.SeatIds == null || !model.SeatIds.Any())
                {
                    return Json(new { success = false, message = "Thiếu thông tin ghế" });
                }

                if (model.ShowTimeId == Guid.Empty)
                {
                    return Json(new { success = false, message = "Thiếu thông tin suất chiếu" });
                }

                var result = await _bookingService.ConfirmAdminBookingAsync(model);
                
                if (result.Success && result.Data != null)
                {
                    return Json(new { success = true, data = result.Data });
                }
                else
                {
                    return Json(new { success = false, message = result.Message ?? "Không thể xác nhận đặt vé" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in ConfirmAdminBooking");
                return Json(new { success = false, message = "Có lỗi xảy ra khi xác nhận đặt vé" });
            }
        }

        /// <summary>
        /// Lấy thông tin chi tiết để xác nhận đặt vé (AC-01)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetBookingConfirmationDetail(Guid showTimeId, string seatIds, string memberId)
        {
            try
            {
                var seatIdList = seatIds.Split(',').Select(Guid.Parse).ToList();
                var result = await _bookingService.GetBookingConfirmationDetailAsync(showTimeId, seatIdList, memberId);
                
                if (result.Success && result.Data != null)
                {
                    return Json(new { success = true, data = result.Data });
                }
                else
                {
                    return Json(new { success = false, message = result.Message ?? "Không thể tải thông tin đặt vé" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetBookingConfirmationDetail");
                return Json(new { success = false, message = "Có lỗi xảy ra khi tải thông tin đặt vé" });
            }
        }

        /// <summary>
        /// Xác nhận đặt vé với tùy chọn chuyển đổi điểm (AC-02, AC-03, AC-04, AC-05)
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> ConfirmBookingWithScore([FromBody] BookingConfirmWithScoreViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                    return Json(new { success = false, message = "Dữ liệu không hợp lệ: " + string.Join(", ", errors) });
                }

                var result = await _bookingService.ConfirmBookingWithScoreAsync(model);
                
                if (result.Success && result.Data != null)
                {
                    return Json(new { success = true, data = result.Data });
                }
                else
                {
                    return Json(new { success = false, message = result.Message ?? "Không thể xác nhận đặt vé" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in ConfirmBookingWithScore");
                return Json(new { success = false, message = "Có lỗi xảy ra khi xác nhận đặt vé" });
            }
        }
    }
}
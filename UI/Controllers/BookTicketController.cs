using Microsoft.AspNetCore.Mvc;
using UI.Areas.BookingManagement.Services; // Service quản lý đặt vé
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
                    var dates = datesResult.Data as IEnumerable<DateTime>;
                    if (dates != null)
                    {
                        var formattedDates = dates.Select(d => d.ToString("yyyy-MM-dd")).ToList();
                        return Json(new { success = true, dates = formattedDates });
                    }
                    else
                    {
                        return Json(new { success = false, message = "Dữ liệu ngày chiếu không hợp lệ." });
                    }
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

    }
}
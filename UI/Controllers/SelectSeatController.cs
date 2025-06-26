using Microsoft.AspNetCore.Mvc;
using UI.Areas.BookingManagement.Services;
using UI.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace UI.Controllers
{
    public class SelectSeatController : Controller
    {
        private readonly IBookingManagementUIService _bookingService;
        private readonly ILogger<SelectSeatController> _logger;

        public SelectSeatController(IBookingManagementUIService bookingService,
                                  ILogger<SelectSeatController> logger)
        {
            _bookingService = bookingService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Index(Guid showtimeId)
        {
            ViewBag.Title = "Chọn Ghế";

            try
            {
                var result = await _bookingService.GetAvailableSeatsAsync(showtimeId);

                if (result.Success && result.Data != null)
                {

                    return View("SelectSeat", result.Data);
                }

                TempData["ErrorMessage"] = result.Message ?? "Không thể tải thông tin ghế.";
                return RedirectToAction("SelectMovieAndShowtime", "BookTicket");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading seat selection page");
                TempData["ErrorMessage"] = "Đã xảy ra lỗi khi tải trang chọn ghế.";
                return RedirectToAction("SelectMovieAndShowtime", "BookTicket");
            }
        }

        [HttpPost]
        public async Task<IActionResult> ValidateSeats(SeatSelectionViewModel model)
        {
            try
            {
                if (model.SelectedSeatIds == null || !model.SelectedSeatIds.Any())
                {
                    return Json(new
                    {
                        success = false,
                        message = "Vui lòng chọn ít nhất một ghế."
                    });
                }

                var result = await _bookingService.ValidateSeatsAsync(
                    model.ShowtimeId, model.SelectedSeatIds);

                if (result.Success && result.Data != null)
                {
                    if (result.Data.IsValid)
                    {
                        return Json(new
                        {
                            success = true,
                            isValid = true,
                            message = result.Data.Message,
                            selectedCount = result.Data.SelectedSeatCount
                        });
                    }

                    return Json(new
                    {
                        success = true,
                        isValid = false,
                        message = result.Data.Message,
                        selectedCount = result.Data.SelectedSeatCount
                    });
                }

                return Json(new
                {
                    success = false,
                    message = result.Message ?? "Không thể xác thực ghế."
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating seats");
                return Json(new
                {
                    success = false,
                    message = "Đã xảy ra lỗi khi xác thực ghế."
                });
            }
        }
    }
}
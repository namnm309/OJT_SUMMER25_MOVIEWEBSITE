using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UI.Areas.ShowtimeManagement.Models;
using UI.Areas.ShowtimeManagement.Services;
using UI.Services;

namespace UI.Areas.ShowtimeManagement.Controllers
{
    [Area("ShowtimeManagement")]
    [Authorize(Roles = "Admin,Staff,2,3")]
    public class ShowtimesController : Controller
    {
        private readonly IApiService _apiService;
        private readonly IShowtimeService _showtimeService;

        public ShowtimesController(IApiService apiService, IShowtimeService showtimeService)
        {
            _apiService = apiService;
            _showtimeService = showtimeService;
        }

        // GET: ShowtimeManagement/Showtimes
        public async Task<IActionResult> Index()
        {
            var viewModel = new ShowtimePageViewModel
            {
                CurrentWeek = GetCurrentWeek(),
                Showtimes = await _showtimeService.GetShowtimesForWeekAsync(DateTime.Now),
                Movies = await _showtimeService.GetActiveMoviesAsync(),
                CinemaRooms = await _showtimeService.GetCinemaRoomsAsync()
            };

            return View(viewModel);
        }

        // GET: ShowtimeManagement/Showtimes/GetWeeklyData
        [HttpGet]
        public async Task<IActionResult> GetWeeklyData(DateTime startDate)
        {
            var showtimes = await _showtimeService.GetShowtimesForWeekAsync(startDate);
            return Json(new { success = true, data = showtimes });
        }

        // GET: ShowtimeManagement/Showtimes/Create
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var model = new CreateShowtimeViewModel
            {
                Movies = await _showtimeService.GetActiveMoviesAsync(),
                CinemaRooms = await _showtimeService.GetCinemaRoomsAsync(),
                ShowDate = DateTime.Today
            };
            return PartialView("_CreateModal", model);
        }

        // POST: ShowtimeManagement/Showtimes/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateShowtimeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "Dữ liệu không hợp lệ" });
            }

            var result = await _showtimeService.CreateShowtimeAsync(model);
            return Json(result);
        }

        // GET: ShowtimeManagement/Showtimes/Edit/5
        [HttpGet]
        public async Task<IActionResult> Edit(Guid id)
        {
            var showtime = await _showtimeService.GetShowtimeByIdAsync(id);
            if (showtime == null)
            {
                return NotFound();
            }

            var model = new EditShowtimeViewModel
            {
                Id = showtime.Id,
                MovieId = showtime.MovieId,
                CinemaRoomId = showtime.CinemaRoomId,
                ShowDate = showtime.ShowDate,
                StartTime = showtime.StartTime,
                Price = showtime.Price,
                Movies = await _showtimeService.GetActiveMoviesAsync(),
                CinemaRooms = await _showtimeService.GetCinemaRoomsAsync()
            };

            return PartialView("_EditModal", model);
        }

        // POST: ShowtimeManagement/Showtimes/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditShowtimeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "Dữ liệu không hợp lệ" });
            }

            var result = await _showtimeService.UpdateShowtimeAsync(model);
            return Json(result);
        }

        // POST: ShowtimeManagement/Showtimes/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Guid id)
        {
            var result = await _showtimeService.DeleteShowtimeAsync(id);
            return Json(result);
        }

        // GET: ShowtimeManagement/Showtimes/CheckConflict
        [HttpGet]
        public async Task<IActionResult> CheckConflict(Guid cinemaRoomId, DateTime showDate, TimeSpan startTime, int duration, Guid? excludeId = null)
        {
            var hasConflict = await _showtimeService.CheckScheduleConflictAsync(cinemaRoomId, showDate, startTime, duration, excludeId);
            return Json(new { hasConflict });
        }

        private WeekInfo GetCurrentWeek()
        {
            var today = DateTime.Today;
            var startOfWeek = today.AddDays(-(int)today.DayOfWeek + 1); // Monday
            var endOfWeek = startOfWeek.AddDays(6); // Sunday

            return new WeekInfo
            {
                StartDate = startOfWeek,
                EndDate = endOfWeek,
                WeekNumber = GetWeekNumber(today)
            };
        }

        private int GetWeekNumber(DateTime date)
        {
            var firstDayOfYear = new DateTime(date.Year, 1, 1);
            var daysOffset = (int)firstDayOfYear.DayOfWeek - 1;
            var firstWeekDay = firstDayOfYear.AddDays(-daysOffset);
            var weekNumber = (int)Math.Ceiling((date - firstWeekDay).TotalDays / 7);
            return weekNumber;
        }
    }
} 
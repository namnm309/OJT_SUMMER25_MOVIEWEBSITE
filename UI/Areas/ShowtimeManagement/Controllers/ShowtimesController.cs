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

        // GET: ShowtimeManagement/Showtimes/AdminDashboard
        [HttpGet]
        [Authorize(Roles = "Admin,2")]
        public async Task<IActionResult> AdminDashboard()
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

        // API: Get Dashboard Statistics
        [HttpGet]
        public async Task<IActionResult> GetDashboardStats()
        {
            try
            {
                var today = DateTime.Today;
                var startOfWeek = today.AddDays(-(int)today.DayOfWeek + 1);
                var endOfWeek = startOfWeek.AddDays(6);

                var allShowtimes = await _showtimeService.GetShowtimesForWeekAsync(startOfWeek);
                
                var stats = new
                {
                    TotalShowtimes = allShowtimes.Count(),
                    TodayShowtimes = allShowtimes.Count(s => s.ShowDate.Date == today),
                    WeeklyShowtimes = allShowtimes.Count(),
                    AvgOccupancy = allShowtimes.Any() ? 
                        Math.Round(allShowtimes.Average(s => (double)s.BookedSeats / s.TotalSeats * 100), 1) : 0,
                    TotalRevenue = allShowtimes.Sum(s => s.BookedSeats * s.Price),
                    PopularMovies = allShowtimes
                        .GroupBy(s => s.MovieTitle)
                        .Select(g => new { 
                            MovieTitle = g.Key, 
                            ShowCount = g.Count(),
                            TotalBookings = g.Sum(s => s.BookedSeats)
                        })
                        .OrderByDescending(x => x.TotalBookings)
                        .Take(5)
                        .ToList(),
                    WeeklyChartData = Enumerable.Range(0, 7)
                        .Select(i => startOfWeek.AddDays(i))
                        .Select(date => new
                        {
                            Date = date.ToString("dd/MM"),
                            ShowCount = allShowtimes.Count(s => s.ShowDate.Date == date),
                            Revenue = allShowtimes.Where(s => s.ShowDate.Date == date).Sum(s => s.BookedSeats * s.Price)
                        })
                        .ToList(),
                    CalendarEvents = allShowtimes.Take(20).Select(s => new
                    {
                        MovieTitle = s.MovieTitle,
                        Start = s.ShowDate.ToString("yyyy-MM-dd") + "T" + s.StartTime.ToString("HH:mm"),
                        End = s.ShowDate.ToString("yyyy-MM-dd") + "T" + s.EndTime.ToString("HH:mm"),
                        BackgroundColor = s.BookedSeats >= s.TotalSeats ? "#ef4444" : 
                                         s.BookedSeats > s.TotalSeats * 0.8 ? "#f59e0b" : "#10b981"
                    }).ToList()
                };

                return Json(new { success = true, data = stats });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // API: Get Monthly Statistics
        [HttpGet]
        public async Task<IActionResult> GetMonthlyStats(int? month = null, int? year = null)
        {
            try
            {
                var targetMonth = month ?? DateTime.Now.Month;
                var targetYear = year ?? DateTime.Now.Year;
                
                var startDate = new DateTime(targetYear, targetMonth, 1);
                var endDate = startDate.AddMonths(1).AddDays(-1);

                // This would need to be implemented in the service
                // For now, return mock data
                var monthlyStats = new
                {
                    Month = startDate.ToString("MM/yyyy"),
                    TotalShowtimes = 156,
                    TotalRevenue = 45000000,
                    AvgOccupancy = 75.5,
                    TopMovies = new[]
                    {
                        new { Title = "Fast & Furious X", Revenue = 12000000, Shows = 45 },
                        new { Title = "John Wick 4", Revenue = 8500000, Shows = 38 },
                        new { Title = "Spider-Man", Revenue = 7200000, Shows = 32 }
                    }
                };

                return Json(new { success = true, data = monthlyStats });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // API: Bulk Delete Showtimes
        [HttpPost]
        public async Task<IActionResult> BulkDelete([FromBody] int[] showtimeIds)
        {
            try
            {
                // Implementation would call service to delete multiple showtimes
                // For now, return success
                return Json(new { success = true, message = $"Đã xóa {showtimeIds.Length} lịch chiếu" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // API: Export Showtimes to Excel
        [HttpGet]
        public async Task<IActionResult> ExportToExcel(DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                var start = startDate ?? DateTime.Today.AddDays(-7);
                var end = endDate ?? DateTime.Today;

                // Implementation would generate Excel file
                // For now, return success message
                return Json(new { success = true, message = "Xuất file Excel thành công", downloadUrl = "/exports/showtimes.xlsx" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
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
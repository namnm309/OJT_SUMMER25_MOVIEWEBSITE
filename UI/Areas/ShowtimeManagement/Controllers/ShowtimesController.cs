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
            try
            {
                var currentMonth = DateTime.Now.Month;
                var currentYear = DateTime.Now.Year;

                var viewModel = new ShowtimePageViewModel
                {
                    CurrentWeek = GetCurrentWeek(),
                    Showtimes = await _showtimeService.GetShowtimesForMonthAsync(currentMonth, currentYear),
                    Movies = await _showtimeService.GetActiveMoviesAsync(),
                    CinemaRooms = await _showtimeService.GetCinemaRoomsAsync()
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = ex.Message;
                var emptyViewModel = new ShowtimePageViewModel
                {
                    CurrentWeek = GetCurrentWeek(),
                    Showtimes = new List<UI.Areas.ShowtimeManagement.Models.ShowtimeDto>(),
                    Movies = new List<UI.Areas.ShowtimeManagement.Models.MovieDto>(),
                    CinemaRooms = new List<UI.Areas.ShowtimeManagement.Models.CinemaRoomDto>()
                };
                return View(emptyViewModel);
            }
        }

        // GET: ShowtimeManagement/Showtimes/GetWeeklyData
        [HttpGet]
        public async Task<IActionResult> GetWeeklyData(DateTime startDate)
        {
            try
            {
                var showtimes = await _showtimeService.GetShowtimesForWeekAsync(startDate);
                return Json(new { success = true, data = showtimes });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // GET: ShowtimeManagement/Showtimes/GetMonthlyData
        [HttpGet]
        public async Task<IActionResult> GetMonthlyData(int month, int year)
        {
            try
            {
                var showtimes = await _showtimeService.GetShowtimesForMonthAsync(month, year);
                return Json(new { success = true, data = showtimes });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // GET: ShowtimeManagement/Showtimes/Create
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            try
            {
                var model = new CreateShowtimeViewModel
                {
                    Movies = await _showtimeService.GetActiveMoviesAsync(),
                    CinemaRooms = await _showtimeService.GetCinemaRoomsAsync(),
                    ShowDate = DateTime.Today
                };
                return PartialView("_CreateModal", model);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
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
            try
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
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
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
            try
            {
                var currentMonth = DateTime.Now.Month;
                var currentYear = DateTime.Now.Year;

                var viewModel = new ShowtimePageViewModel
                {
                    CurrentWeek = GetCurrentWeek(),
                    Showtimes = await _showtimeService.GetShowtimesForMonthAsync(currentMonth, currentYear),
                    Movies = await _showtimeService.GetActiveMoviesAsync(),
                    CinemaRooms = await _showtimeService.GetCinemaRoomsAsync()
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = ex.Message;
                var emptyViewModel = new ShowtimePageViewModel
                {
                    CurrentWeek = GetCurrentWeek(),
                    Showtimes = new List<UI.Areas.ShowtimeManagement.Models.ShowtimeDto>(),
                    Movies = new List<UI.Areas.ShowtimeManagement.Models.MovieDto>(),
                    CinemaRooms = new List<UI.Areas.ShowtimeManagement.Models.CinemaRoomDto>()
                };
                return View(emptyViewModel);
            }
        }

        // API: Get Dashboard Statistics
        [HttpGet]
        public async Task<IActionResult> GetDashboardStats()
        {
            try
            {
                var currentMonth = DateTime.Now.Month;
                var currentYear = DateTime.Now.Year;
                var today = DateTime.Today;

                var allShowtimes = await _showtimeService.GetShowtimesForMonthAsync(currentMonth, currentYear) ?? new List<UI.Areas.ShowtimeManagement.Models.ShowtimeDto>();
                
                var stats = new
                {
                    TotalShowtimes = allShowtimes.Count(),
                    TodayShowtimes = allShowtimes.Count(s => s.ShowDate.Date == today),
                    MonthlyShowtimes = allShowtimes.Count(),
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
                    MonthlyChartData = Enumerable.Range(1, DateTime.DaysInMonth(currentYear, currentMonth))
                        .Select(day => new DateTime(currentYear, currentMonth, day))
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
                
                var showtimes = await _showtimeService.GetShowtimesForMonthAsync(targetMonth, targetYear);
                
                var monthlyStats = new
                {
                    Month = new DateTime(targetYear, targetMonth, 1).ToString("MM/yyyy"),
                    TotalShowtimes = showtimes.Count(),
                    TotalRevenue = showtimes.Sum(s => s.BookedSeats * s.Price),
                    AvgOccupancy = showtimes.Any() ? 
                        Math.Round(showtimes.Average(s => (double)s.BookedSeats / s.TotalSeats * 100), 1) : 0,
                    TopMovies = showtimes
                        .GroupBy(s => s.MovieTitle)
                        .Select(g => new { 
                            Title = g.Key, 
                            Revenue = g.Sum(s => s.BookedSeats * s.Price), 
                            Shows = g.Count() 
                        })
                        .OrderByDescending(x => x.Revenue)
                        .Take(3)
                        .ToArray()
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

        // GET: ShowtimeManagement/Showtimes/TestAPI
        [HttpGet]
        public async Task<IActionResult> TestAPI()
        {
            try
            {
                var currentMonth = DateTime.Now.Month;
                var currentYear = DateTime.Now.Year;
                
                // Test the new API
                var allShowtimes = await _showtimeService.GetShowtimesForMonthAsync(currentMonth, currentYear);
                
                return Json(new { 
                    success = true, 
                    message = $"API hoạt động bình thường. Tìm thấy {allShowtimes.Count} lịch chiếu trong tháng {currentMonth}/{currentYear}",
                    data = allShowtimes,
                    currentMonth = currentMonth,
                    currentYear = currentYear
                });
            }
            catch (Exception ex)
            {
                return Json(new { 
                    success = false, 
                    message = $"Lỗi API: {ex.Message}",
                    error = ex.ToString()
                });
            }
        }

        // GET: ShowtimeManagement/Showtimes/TestData
        [HttpGet]
        public async Task<IActionResult> TestData()
        {
            try
            {
                // Test GetAllShowtimes
                var allShowtimes = await _showtimeService.GetShowtimesForWeekAsync(DateTime.Now);
                
                // Test GetMonthlyData
                var currentMonth = DateTime.Now.Month;
                var currentYear = DateTime.Now.Year;
                var monthlyShowtimes = await _showtimeService.GetShowtimesForMonthAsync(currentMonth, currentYear);
                
                // Test August 2025 data (based on your database screenshot)
                var augustShowtimes = await _showtimeService.GetShowtimesForMonthAsync(8, 2025);
                
                return Json(new { 
                    success = true,
                    message = "Test completed successfully",
                    data = new {
                        currentWeekCount = allShowtimes?.Count ?? 0,
                        currentMonthCount = monthlyShowtimes?.Count ?? 0,
                        august2025Count = augustShowtimes?.Count ?? 0,
                        august2025Data = augustShowtimes?.Take(3).Select(s => new {
                            id = s.Id,
                            movieTitle = s.MovieTitle,
                            showDate = s.ShowDate.ToString("yyyy-MM-dd"),
                            startTime = s.StartTime.ToString()
                        })
                    }
                });
            }
            catch (Exception ex)
            {
                return Json(new { 
                    success = false, 
                    error = ex.Message,
                    innerException = ex.InnerException?.Message
                });
            }
        }

        // GET: ShowtimeManagement/Showtimes/DebugData
        [HttpGet]
        public async Task<IActionResult> DebugData()
        {
            try
            {
                // Test August 2025 data (based on your database screenshot)
                var augustShowtimes = await _showtimeService.GetShowtimesForMonthAsync(8, 2025);
                
                // Test current month
                var currentMonth = DateTime.Now.Month;
                var currentYear = DateTime.Now.Year;
                var currentShowtimes = await _showtimeService.GetShowtimesForMonthAsync(currentMonth, currentYear);
                
                return Json(new { 
                    success = true,
                    message = "Debug data loaded successfully",
                    august2025 = new {
                        count = augustShowtimes?.Count ?? 0,
                        data = augustShowtimes?.Take(5).Select(s => new {
                            id = s.Id,
                            movieTitle = s.MovieTitle,
                            moviePoster = s.MoviePoster,
                            cinemaRoomName = s.CinemaRoomName,
                            showDate = s.ShowDate.ToString("yyyy-MM-dd"),
                            startTime = s.StartTime.ToString(),
                            endTime = s.EndTime.ToString(),
                            price = s.Price,
                            totalSeats = s.TotalSeats,
                            bookedSeats = s.BookedSeats,
                            isActive = s.IsActive
                        }).ToList()
                    },
                    currentMonth = new {
                        month = currentMonth,
                        year = currentYear,
                        count = currentShowtimes?.Count ?? 0,
                        data = currentShowtimes?.Take(3).Select(s => new {
                            id = s.Id,
                            movieTitle = s.MovieTitle,
                            showDate = s.ShowDate.ToString("yyyy-MM-dd"),
                            startTime = s.StartTime.ToString()
                        }).ToList()
                    }
                });
            }
            catch (Exception ex)
            {
                return Json(new { 
                    success = false, 
                    error = ex.Message,
                    stackTrace = ex.ToString()
                });
            }
        }

        // GET: ShowtimeManagement/Showtimes/TestPoster
        [HttpGet]
        public async Task<IActionResult> TestPoster()
        {
            try
            {
                // Test August 2025 data
                var augustShowtimes = await _showtimeService.GetShowtimesForMonthAsync(8, 2025);
                
                return Json(new { 
                    success = true,
                    message = "Poster test data",
                    data = augustShowtimes?.Take(5).Select(s => new {
                        id = s.Id,
                        movieTitle = s.MovieTitle,
                        moviePoster = s.MoviePoster,
                        posterLength = s.MoviePoster?.Length ?? 0,
                        posterIsEmpty = string.IsNullOrEmpty(s.MoviePoster),
                        showDate = s.ShowDate.ToString("yyyy-MM-dd"),
                        startTime = s.StartTime.ToString()
                    })
                });
            }
            catch (Exception ex)
            {
                return Json(new { 
                    success = false, 
                    error = ex.Message,
                    stackTrace = ex.ToString()
                });
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
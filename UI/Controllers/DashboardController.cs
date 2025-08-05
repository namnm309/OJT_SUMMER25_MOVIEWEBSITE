using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text.Json;
using UI.Services;
using System.Collections.Generic;
using UI.Models;

namespace UI.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly IApiService _apiService;

        public DashboardController(IApiService apiService)
        {
            _apiService = apiService;
        }

        public IActionResult Index()
        {
            // Lấy thông tin user role từ claims
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
            
            // Chuyển hướng dựa trên vai trò
            if (userRole == "Admin" || userRole == "2")
                return RedirectToAction("AdminDashboard");
            else if (userRole == "Staff" || userRole == "3")
                return RedirectToAction("StaffDashboard");
            else
                return RedirectToAction("MemberDashboard");
        }

        [Authorize(Roles = "Admin,2")]
        public async Task<IActionResult> AdminDashboard()
        {
            // Lấy thông tin user từ claims
            var userId = User.FindFirst("UserId")?.Value;
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
            var userName = User.FindFirst("Username")?.Value ?? User.Identity?.Name;
            var fullName = User.FindFirst("FullName")?.Value;
            var email = User.FindFirst("Email")?.Value;
            var isActive = User.FindFirst("IsActive")?.Value;
            var createdAt = User.FindFirst("CreatedAt")?.Value;

            ViewBag.UserId = userId;
            ViewBag.UserName = userName;
            ViewBag.FullName = fullName;
            ViewBag.Email = email;
            ViewBag.Role = userRole;
            ViewBag.IsActive = isActive == "True";
            
            // Format ngày tạo
            if (DateTime.TryParse(createdAt, out DateTime created))
            {
                ViewBag.CreatedAt = created.ToString("dd/MM/yyyy HH:mm");
            }
            else
            {
                ViewBag.CreatedAt = "N/A";
            }

            try
            {
                // 1. Tổng số thành viên và growth
                var usersResp = await _apiService.GetAsync<JsonElement>("/api/User/count");
                var usersGrowthResp = await _apiService.GetAsync<JsonElement>("/api/User/growth");
                
                if (usersResp.Success)
                {
                    ViewBag.TotalUsers = usersResp.Data.GetProperty("count").GetInt32().ToString("N0");
                }
                else
                {
                    ViewBag.TotalUsers = "N/A";
                }

                if (usersGrowthResp.Success)
                {
                    var growth = usersGrowthResp.Data.GetProperty("growth").GetDouble();
                    ViewBag.UsersGrowth = growth >= 0 ? $"+{growth:F1}%" : $"{growth:F1}%";
                    ViewBag.UsersGrowthPositive = growth >= 0;
                }
                else
                {
                    ViewBag.UsersGrowth = "N/A";
                    ViewBag.UsersGrowthPositive = true;
                }

                // 2. Tổng số phim và growth
                var moviesResp = await _apiService.GetAsync<JsonElement>("/api/movie/count");
                var moviesGrowthResp = await _apiService.GetAsync<JsonElement>("/api/movie/growth");
                
                if (moviesResp.Success)
                {
                    ViewBag.TotalMovies = moviesResp.Data.GetProperty("count").GetInt32().ToString("N0");
                }
                else
                {
                    ViewBag.TotalMovies = "N/A";
                }

                if (moviesGrowthResp.Success)
                {
                    var growth = moviesGrowthResp.Data.GetProperty("growth").GetDouble();
                    ViewBag.MoviesGrowth = growth >= 0 ? $"+{growth:F1}%" : $"{growth:F1}%";
                    ViewBag.MoviesGrowthPositive = growth >= 0;
                }
                else
                {
                    ViewBag.MoviesGrowth = "N/A";
                    ViewBag.MoviesGrowthPositive = true;
                }

                // 3. Số vé đã đặt hôm nay và growth
                var todayBookingsResp = await _apiService.GetAsync<JsonElement>("/api/booking-ticket/today-count");
                var bookingsGrowthResp = await _apiService.GetAsync<JsonElement>("/api/booking-ticket/booking-growth");
                
                if (todayBookingsResp.Success)
                {
                    ViewBag.TotalBookings = todayBookingsResp.Data.GetProperty("count").GetInt32().ToString("N0");
                }
                else
                {
                    ViewBag.TotalBookings = "N/A";
                }

                if (bookingsGrowthResp.Success)
                {
                    var growth = bookingsGrowthResp.Data.GetProperty("growth").GetDouble();
                    ViewBag.BookingsGrowth = growth >= 0 ? $"+{growth:F1}%" : $"{growth:F1}%";
                    ViewBag.BookingsGrowthPositive = growth >= 0;
                }
                else
                {
                    ViewBag.BookingsGrowth = "N/A";
                    ViewBag.BookingsGrowthPositive = true;
                }

                // 4. Doanh thu hôm nay và growth
                var todayRevenueResp = await _apiService.GetAsync<JsonElement>("/api/booking-ticket/today-revenue");
                var revenueGrowthResp = await _apiService.GetAsync<JsonElement>("/api/booking-ticket/revenue-growth");
                
                if (todayRevenueResp.Success)
                {
                    var revenue = todayRevenueResp.Data.GetProperty("revenue").GetDecimal();
                    ViewBag.TodayRevenue = revenue.ToString("N0") + " VNĐ";
                }
                else
                {
                    ViewBag.TodayRevenue = "N/A";
                }

                if (revenueGrowthResp.Success)
                {
                    var growth = revenueGrowthResp.Data.GetProperty("growth").GetDouble();
                    ViewBag.RevenueGrowth = growth >= 0 ? $"+{growth:F1}%" : $"{growth:F1}%";
                    ViewBag.RevenueGrowthPositive = growth >= 0;
                }
                else
                {
                    ViewBag.RevenueGrowth = "N/A";
                    ViewBag.RevenueGrowthPositive = true;
                }

                // 5. Số nhiệm vụ cần xử lý (pending bookings)
                var pendingTasksResp = await _apiService.GetAsync<JsonElement>("/api/booking-ticket/pending-count");
                if (pendingTasksResp.Success)
                {
                    ViewBag.PendingTasks = pendingTasksResp.Data.GetProperty("count").GetInt32().ToString("N0");
                }
                else
                {
                    ViewBag.PendingTasks = "N/A";
                }
            }
            catch (Exception ex)
            {
                // Log error but don't crash the page
                ViewBag.TotalUsers = "N/A";
                ViewBag.TotalMovies = "N/A";
                ViewBag.TotalBookings = "N/A";
                ViewBag.TodayRevenue = "N/A";
                ViewBag.PendingTasks = "N/A";
                
                // Set default growth values
                ViewBag.UsersGrowth = "N/A";
                ViewBag.UsersGrowthPositive = true;
                ViewBag.MoviesGrowth = "N/A";
                ViewBag.MoviesGrowthPositive = true;
                ViewBag.BookingsGrowth = "N/A";
                ViewBag.BookingsGrowthPositive = true;
                ViewBag.RevenueGrowth = "N/A";
                ViewBag.RevenueGrowthPositive = true;
            }
            
            return View("AdminDashboard");
        }

        [Authorize(Roles = "Staff,3")]
        public IActionResult StaffDashboard()
        {
            // Lấy thông tin user từ claims
            var userId = User.FindFirst("UserId")?.Value;
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
            var userName = User.FindFirst("Username")?.Value ?? User.Identity?.Name;
            var fullName = User.FindFirst("FullName")?.Value;
            var email = User.FindFirst("Email")?.Value;
            var isActive = User.FindFirst("IsActive")?.Value;
            var createdAt = User.FindFirst("CreatedAt")?.Value;


            ViewBag.UserId = userId;
            ViewBag.UserName = userName;
            ViewBag.FullName = fullName;
            ViewBag.Email = email;
            ViewBag.Role = userRole;
            ViewBag.IsActive = isActive == "True";
            
            // Format ngày tạo
            if (DateTime.TryParse(createdAt, out DateTime created))
            {
                ViewBag.CreatedAt = created.ToString("dd/MM/yyyy HH:mm");
            }
            else
            {
                ViewBag.CreatedAt = "N/A";
            }


            ViewBag.TodayTickets = "N/A";
            ViewBag.TotalCustomers = "N/A";
            ViewBag.WorkingHours = "N/A";
            
            return View("StaffDashboard");
        }

        [Authorize(Roles = "Member,1")]
        public async Task<IActionResult> MemberDashboard()
        {
            try
            {
                // 1) Profile
                var profileResp = await _apiService.GetAsync<JsonElement>("/api/user/profile");
                if (profileResp.Success && profileResp.Data.ValueKind != JsonValueKind.Undefined)
                {
                    var p = profileResp.Data;
                    ViewBag.UserId = Guid.Parse(p.GetProperty("userId").ToString());
                    ViewBag.UserName = p.GetProperty("username").GetString();
                    ViewBag.FullName = p.GetProperty("fullName").GetString();
                    ViewBag.Email = p.GetProperty("email").GetString();
                    ViewBag.Role = p.GetProperty("role").GetString();
                    double scoreVal = p.TryGetProperty("score", out var sc) && sc.ValueKind == JsonValueKind.Number ? sc.GetDouble() : 0;
                    ViewBag.Score = scoreVal;
                    // Compute next tier (Silver 2000, Gold 5000, Platinum 10000)
                    double nextTier = scoreVal < 2000 ? 2000 : (scoreVal < 5000 ? 5000 : (scoreVal < 10000 ? 10000 : Math.Ceiling(scoreVal/5000)*5000));
                    ViewBag.PointsNeeded = nextTier - scoreVal;
                    ViewBag.ProgressPercent = (nextTier == 0) ? 0 : Math.Min(100, Math.Round(scoreVal / nextTier * 100, 1));
                    ViewBag.Phone = p.TryGetProperty("phone", out var ph) ? ph.GetString() : "";
                    ViewBag.IdentityCard = p.TryGetProperty("identityCard", out var ic) ? ic.GetString() : "";
                    ViewBag.Address = p.TryGetProperty("address", out var ad) ? ad.GetString() : "";
                    ViewBag.BirthDate = p.TryGetProperty("birthDate", out var bd) && bd.ValueKind == JsonValueKind.String ? DateTime.Parse(bd.GetString()).ToString("yyyy-MM-dd") : "";
                    ViewBag.Gender = p.TryGetProperty("gender", out var gd) ? gd.GetString() : "";
                    ViewBag.IsActive = p.TryGetProperty("isActive", out var ia) && ia.GetBoolean();
                    ViewBag.CreatedAt = p.TryGetProperty("createdAt", out var ca) ? ca.GetDateTime().ToString("dd/MM/yyyy HH:mm") : "N/A";
                }

                // 2) Booking count
                var bookingResp = await _apiService.GetAsync<JsonElement>("/api/v1/booking-ticket/user-bookings-count");
                if (bookingResp.Success)
                {
                    ViewBag.TicketCount = bookingResp.Data.GetProperty("count").GetInt32();
                }

                // 3) Vouchers of user
                var voucherResp = await _apiService.GetAsync<List<JsonElement>>("/api/v1/promotions/my");
                if (voucherResp.Success)
                {
                    ViewBag.UserVouchers = voucherResp.Data;
                }

                // 4) User Bookings History
                var historyResp = await _apiService.GetAsync<List<JsonElement>>("/api/v1/booking-ticket/user-bookings");
                if(historyResp.Success)
                {
                    ViewBag.BookingHistory = historyResp.Data;
                }
            }
            catch(Exception ex)
            {
                // Log omitted for brevity
            }

            return View("MemberDashboard");
        }

        [Authorize(Roles = "Admin,2")]
        public IActionResult AdminPanel()
        {
            return View();
        }

        [Authorize(Roles = "Admin,Staff,2,3")]
        public IActionResult StaffPanel()
        {
            return View();
        }


        [Authorize(Roles = "Admin,Staff,2,3")]
        public IActionResult BookTicket()
        {
            return View();
        }

        [Authorize(Roles = "Admin,Staff,2,3")]
        public IActionResult BookTicketStep1()
        {
            return View();
        }

        [Authorize(Roles = "Admin,Staff,2,3")]
        public IActionResult BookTicketStep2()
        {
            return View();
        }

        [Authorize(Roles = "Admin,Staff,2,3")]
        public IActionResult BookTicketStep3()
        {
            return View();
        }

        [HttpPut]
        public async Task<IActionResult> EditProfile([FromBody] EditProfileModel model)
        {
            try
            {
                var data = new
                {
                    Email = model.Email,
                    FullName = model.FullName,
                    Phone = model.Phone,
                    IdentityCard = model.IdentityCard,
                    Address = model.Address,
                    BirthDate = model.BirthDate,
                    Gender = model.Gender
                };

                var response = await _apiService.PutAsync<object>("/api/user/profile", data);

                if (response.Success)
                {
                    return Json(new { success = true, message = "Profile updated successfully" });
                }
                else
                {
                    return Json(new { success = false, message = response.Message });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error: {ex.Message}" });
            }
        }
    }
}
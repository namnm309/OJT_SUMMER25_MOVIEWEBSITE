using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text.Json;
using UI.Services;
using System.Collections.Generic;

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
        public IActionResult AdminDashboard()
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


            ViewBag.TotalUsers = "N/A";
            ViewBag.TotalMovies = "N/A"; 
            ViewBag.TotalBookings = "N/A";
            ViewBag.TodayRevenue = "N/A";
            ViewBag.PendingTasks = "N/A";
            
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
                var voucherResp = await _apiService.GetAsync<List<JsonElement>>("/api/v1/vouchers/my");
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
    }
}
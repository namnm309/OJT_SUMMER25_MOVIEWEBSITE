using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text.Json;
using UI.Services;

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
            // Lấy thông tin user từ claims
            var userId = User.FindFirst("UserId")?.Value;
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
            var userName = User.FindFirst("Username")?.Value ?? User.Identity?.Name;
            var fullName = User.FindFirst("FullName")?.Value;
            var email = User.FindFirst("Email")?.Value;
            var isActive = User.FindFirst("IsActive")?.Value;
            var createdAt = User.FindFirst("CreatedAt")?.Value;

            // Pass data to ViewBag
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

            // Add placeholder stats for different roles (sẽ thay bằng real data sau)
            if (userRole == "Admin")
            {
                ViewBag.TotalUsers = "N/A";
                ViewBag.TotalMovies = "N/A"; 
                ViewBag.TotalBookings = "N/A";
                ViewBag.TodayRevenue = "N/A";
                ViewBag.PendingTasks = "N/A";
            }
            else if (userRole == "Staff")
            {
                ViewBag.TodayTickets = "N/A";
                ViewBag.TotalCustomers = "N/A";
                ViewBag.WorkingHours = "N/A";
            }

            return userRole switch
            {
                "2" => View("AdminDashboard"),
                "3" => View("StaffDashboard"), 
                "1" => View("MemberDashboard"),
                "Member" => View("MemberDashboard"),
                "Admin" => View("AdminDashboard"),
                "Staff" => View("StaffDashboard"),
                _ => View("MemberDashboard")
            };
        }

        [Authorize(Roles = "Admin")]
        public IActionResult AdminPanel()
        {
            return View();
        }

        [Authorize(Roles = "Admin,Staff")]
        public IActionResult StaffPanel()
        {
            return View();
        }
    }
}
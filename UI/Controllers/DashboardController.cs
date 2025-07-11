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
        public IActionResult MemberDashboard()
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
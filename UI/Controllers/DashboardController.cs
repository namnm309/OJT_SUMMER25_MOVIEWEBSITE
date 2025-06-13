using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text.Json;

namespace UI.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiBaseUrl;

        public DashboardController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClient = httpClientFactory.CreateClient("ApiClient");
            _apiBaseUrl = configuration["ApiSettings:BaseUrl"] ?? "https://localhost:7285";
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
                "Admin" => View("AdminDashboard"),
                "Staff" => View("StaffDashboard"), 
                "Member" => View("MemberDashboard"),
                _ => View("MemberDashboard")
            };
        }

        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> Members()
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_apiBaseUrl}/api/user/members");
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var members = JsonSerializer.Deserialize<JsonElement[]>(responseContent);
                    
                    ViewBag.Members = members;
                    return View();
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to load members list.";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error loading members: {ex.Message}";
            }

            return View();
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
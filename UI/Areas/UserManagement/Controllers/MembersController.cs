using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Text.Json;
using UI.Services;

namespace UI.Areas.UserManagement.Controllers
{
    [Area("UserManagement")]
    //[Authorize(Roles = "Admin,Staff")] // Tạm thời comment để debug
    [AllowAnonymous]
    public class MembersController : Controller
    {
        private readonly IApiService _apiService;
        private readonly ILogger<MembersController> _logger;

        public MembersController(IApiService apiService, ILogger<MembersController> logger)
        {
            _apiService = apiService;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            ViewData["Title"] = "Quản lý thành viên";

            try
            {
                // Gọi API thông qua ApiService
                var result = await _apiService.GetAsync<JsonElement>("/api/User/members");
                
                if (result.Success && result.Data.TryGetProperty("data", out var dataProp) && dataProp.ValueKind == JsonValueKind.Array)
                {
                    var membersArray = dataProp.EnumerateArray().ToArray();
                    ViewBag.Members = membersArray;
                    
                    // Tính toán số liệu thống kê
                    ViewBag.TotalMembers = membersArray.Length;
                    ViewBag.ActiveMembers = membersArray.Length;
                    ViewBag.StaffMembers = membersArray.Count(m => 
                        m.TryGetProperty("role", out var role) && 
                        (role.ToString() == "Staff" || role.ToString() == "Admin"));
                    
                    var oneWeekAgo = DateTime.UtcNow.AddDays(-7);
                    ViewBag.NewMembers = membersArray.Count(m => 
                        m.TryGetProperty("createdAt", out var createdAt) && 
                        DateTime.TryParse(createdAt.ToString(), out var date) && 
                        date >= oneWeekAgo);
                    
                    return View();
                }
                else
                {
                    TempData["ErrorMessage"] = "Không thể tải danh sách thành viên";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading members");
                TempData["ErrorMessage"] = $"Đã xảy ra lỗi: {ex.Message}";
            }

            // Hiển thị view ngay cả khi có lỗi
            ViewBag.Members = Array.Empty<JsonElement>();
            ViewBag.TotalMembers = 0;
            ViewBag.ActiveMembers = 0;
            ViewBag.StaffMembers = 0;
            ViewBag.NewMembers = 0;
            return View();
        }
    }
} 
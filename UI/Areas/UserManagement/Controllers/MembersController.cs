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

        public async Task<IActionResult> Index(string sortOrder = "asc", string search = "")
        {
            ViewData["Title"] = "Quản lý thành viên";
            ViewBag.CurrentSort = sortOrder;
            ViewBag.Search = search;

            try
            {
                // Gọi API thông qua ApiService với tham số search
                var apiUrl = string.IsNullOrEmpty(search) ? "/api/User/members" : $"/api/User/members?search={Uri.EscapeDataString(search)}";
                var result = await _apiService.GetAsync<JsonElement>(apiUrl);
                
                if (result.Success && result.Data.TryGetProperty("data", out var dataProp) && dataProp.ValueKind == JsonValueKind.Array)
                {
                    var membersList = dataProp.EnumerateArray().ToList();
                    
                    // Sắp xếp theo tên
                    membersList = sortOrder == "desc" 
                        ? membersList.OrderByDescending(m => m.TryGetProperty("fullName", out var name) ? name.ToString() : string.Empty).ToList()
                        : membersList.OrderBy(m => m.TryGetProperty("fullName", out var name) ? name.ToString() : string.Empty).ToList();
                    
                    ViewBag.Members = membersList.ToArray();
                    
                    // Tính toán số liệu thống kê
                    ViewBag.TotalMembers = membersList.Count;
                    ViewBag.ActiveMembers = membersList.Count;
                    ViewBag.StaffMembers = membersList.Count(m => 
                        m.TryGetProperty("role", out var role) && 
                        (role.ToString() == "Staff" || role.ToString() == "Admin"));
                    
                    var oneWeekAgo = DateTime.UtcNow.AddDays(-7);
                    ViewBag.NewMembers = membersList.Count(m => 
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
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Text.Json;
using UI.Services;
using UI.Areas.UserManagement.Models;

namespace UI.Areas.UserManagement.Controllers
{
    [Area("UserManagement")]
    [Authorize]
    public class AccountController : Controller
    {
        private readonly IApiService _apiService;
        private readonly ILogger<AccountController> _logger;

        public AccountController(IApiService apiService, ILogger<AccountController> logger)
        {
            _apiService = apiService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            try
            {
                var result = await _apiService.GetAsync<JsonElement>("/api/user/profile");
                
                if (result.Success && result.Data.ValueKind != JsonValueKind.Undefined)
                {
                    var userProfile = result.Data;
                    var model = new EditProfileViewModel
                    {
                        // Xử lý các kiểu dữ liệu một cách linh hoạt hơn
                        UserId = Guid.Parse(userProfile.GetProperty("userId").ToString()),
                        Username = userProfile.GetProperty("username").GetString() ?? string.Empty,
                        Email = userProfile.GetProperty("email").GetString() ?? string.Empty,
                        FullName = userProfile.GetProperty("fullName").GetString() ?? string.Empty,
                        Phone = userProfile.GetProperty("phone").GetString() ?? string.Empty,
                        IdentityCard = userProfile.GetProperty("identityCard").GetString() ?? string.Empty,
                        Address = userProfile.GetProperty("address").GetString() ?? string.Empty,
                        
                        // Xử lý score có thể là số hoặc chuỗi
                        Score = userProfile.TryGetProperty("score", out var score) 
                            ? (score.ValueKind == JsonValueKind.Number 
                                ? score.GetDouble() 
                                : double.TryParse(score.ToString(), out var scoreValue) ? scoreValue : 0) 
                            : 0,
                        
                        Role = userProfile.TryGetProperty("role", out var role) 
                            ? (role.ValueKind == JsonValueKind.String ? role.GetString() : role.ToString()) 
                            : string.Empty,
                            
                        // Xử lý datetime và các trường tùy chọn
                        BirthDate = userProfile.TryGetProperty("birthDate", out var bd) && bd.ValueKind != JsonValueKind.Null
                            ? (bd.ValueKind == JsonValueKind.String && !string.IsNullOrEmpty(bd.GetString())
                                ? DateTime.TryParse(bd.GetString(), out var bdDate) ? bdDate : null
                                : null)
                            : null,
                            
                        Gender = userProfile.TryGetProperty("gender", out var g) && g.ValueKind != JsonValueKind.Null
                            ? (g.ValueKind == JsonValueKind.Number ? g.GetInt32() : 
                               int.TryParse(g.ToString(), out var genderValue) ? genderValue : 0)
                            : 0,
                            
                        Avatar = userProfile.TryGetProperty("avatar", out var a) && a.ValueKind != JsonValueKind.Null
                            ? (a.ValueKind == JsonValueKind.String ? a.GetString() ?? string.Empty : string.Empty)
                            : string.Empty
                    };

                    return View(model);
                }
                else
                {
                    TempData["ErrorMessage"] = result.Message;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading profile");
                TempData["ErrorMessage"] = $"Error loading profile: {ex.Message}";
            }

            return RedirectToAction("Index", "Dashboard", new { area = "" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Profile(EditProfileViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var editData = new
                {
                    email = model.Email,
                    fullName = model.FullName,
                    phone = model.Phone,
                    identityCard = model.IdentityCard,
                    address = model.Address,
                    birthDate = model.BirthDate,
                    gender = model.Gender,
                    avatar = model.Avatar,
                    newPassword = model.NewPassword,
                    confirmNewPassword = model.ConfirmNewPassword
                };

                var result = await _apiService.PutAsync<JsonElement>("/api/user/profile", editData);

                if (result.Success)
                {
                    TempData["SuccessMessage"] = "Profile updated successfully!";
                    return RedirectToAction("Profile");
                }
                else
                {
                    ModelState.AddModelError("", result.Message);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating profile");
                ModelState.AddModelError("", $"An error occurred: {ex.Message}");
            }

            return View(model);
        }
    }
} 
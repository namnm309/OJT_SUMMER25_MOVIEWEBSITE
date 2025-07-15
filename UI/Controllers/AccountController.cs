using Microsoft.AspNetCore.Mvc;
using UI.Models;
using UI.Areas.UserManagement.Models;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using UI.Services;
using Microsoft.AspNetCore.Http;

namespace UI.Controllers
{
    public class AccountController : Controller
    {
        private readonly IApiService _apiService;

        public AccountController(IApiService apiService)
        {
            _apiService = apiService;
        }

        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Home");
            }

            ViewData["ReturnUrl"] = returnUrl;
            return View(new LoginViewModel { ReturnUrl = returnUrl });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                // Nếu là AJAX request, trả về JSON
                if (Request.Headers["Content-Type"].ToString().Contains("application/json"))
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                    return Json(new { success = false, message = string.Join(", ", errors) });
                }
                return Json(new { success = false, message = "Invalid model state" });
            }

            try
            {
                // Gửi tới API mới dùng JWT
                var loginData = new
                {
                    account = model.Username,
                    password = model.Password
                };

                var result = await _apiService.PostAsync<JsonElement>("https://localhost:7049/api/v1/Auth/Login", loginData);

                if (result.Success && result.Data.ValueKind != JsonValueKind.Undefined)
                {
                    var data = result.Data;

                    // Lấy token
                    JsonElement tokenProp;
                    if (!data.TryGetProperty("token", out tokenProp) &&
                        !data.TryGetProperty("Token", out tokenProp))
                    {
                        // Thử tìm trong field Data / data lồng bên trong
                        JsonElement dataInner;
                        if (data.TryGetProperty("data", out dataInner) || data.TryGetProperty("Data", out dataInner))
                        {
                            if (!dataInner.TryGetProperty("token", out tokenProp) &&
                                !dataInner.TryGetProperty("Token", out tokenProp))
                            {
                                return Json(new { success = false, message = "Token is missing in response" });
                            }
                        }
                        else
                        {
                            return Json(new { success = false, message = "Token is missing in response" });
                        }
                    }

                    var token = tokenProp.GetString();
                    if (string.IsNullOrEmpty(token))
                    {
                        return Json(new { success = false, message = "Token is empty" });
                    }

                    // Lưu vào Session để ApiService sử dụng
                    HttpContext.Session.SetString("JWToken", token);

                    string role;
                    JsonElement rolePropEl;
                    if (data.TryGetProperty("role", out rolePropEl) || data.TryGetProperty("Role", out rolePropEl))
                    {
                        role = rolePropEl.GetString() ?? "member";
                    }
                    else if ((data.TryGetProperty("data", out var dataInnerRole) || data.TryGetProperty("Data", out dataInnerRole)) &&
                             (dataInnerRole.TryGetProperty("role", out rolePropEl) || dataInnerRole.TryGetProperty("Role", out rolePropEl)))
                    {
                        role = rolePropEl.GetString() ?? "member";
                    }
                    else
                    {
                        role = "member";
                    }

                    // RedirectUrl tuỳ API, có thể không có
                    string? redirectUrl = null;
                    // Tự động redirect theo role
                    if (role.ToLower() == "admin" || role == "2")
                        redirectUrl = "/Dashboard/AdminDashboard";
                    else if (role.ToLower() == "staff" || role == "3")
                        redirectUrl = "/Dashboard/StaffDashboard";
                    else
                        redirectUrl = "/";

                    return new JsonResult(new
                    {
                        success = true,
                        token = token,
                        role = role,
                        redirectUrl = redirectUrl
                    });
                }
                else
                {
                    var message = string.IsNullOrEmpty(result.Message) ? "Đăng nhập không thành công." : result.Message;
                    return new JsonResult(new { success = false, message = message });
                }
            }
            catch (Exception ex)
            {
                if (Request.Headers["Content-Type"].ToString().Contains("application/json"))
                {
                    return Json(new { success = false, message = $"An error occurred: {ex.Message}" });
                }
                ModelState.AddModelError("", $"An error occurred: {ex.Message}");
            }

            return View(model);
        }

        [HttpGet]
        public IActionResult Register()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Home");
            }

            return View(new RegisterViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                // Nếu là AJAX request, trả về JSON
                if (Request.Headers["Content-Type"].ToString().Contains("application/json"))
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                    return Json(new { success = false, message = string.Join(", ", errors) });
                }
                return Json(new { success = false, message = "Invalid model state" });
            }

            try
            {
                // Chuyển đổi gender từ string sang enum UserGender
                int genderValue = 1; // Mặc định là Male (1)
                if (!string.IsNullOrEmpty(model.Gender))
                {
                    if (model.Gender.Equals("Nữ", StringComparison.OrdinalIgnoreCase))
                    {
                        genderValue = 2; // Female
                    }
                }

                var registerData = new
                {
                    username = model.Username,
                    password = model.Password,
                    confirmPassword = model.ConfirmPassword,
                    email = model.Email,
                    fullName = model.FullName,
                    phone = model.Phone,
                    identityCard = model.IdentityCard,
                    address = model.Address,
                    birthDate = model.BirthDate,
                    gender = genderValue // Sử dụng giá trị số của enum
                };

                var result = await _apiService.PostAsync<JsonElement>("/api/user/register", registerData);

                if (result.Success)
                {
                    // Nếu là AJAX request
                    if (Request.Headers["Content-Type"].ToString().Contains("application/json"))
                    {
                        return Json(new { success = true, message = "Registration successful! Please login to continue." });
                    }

                    TempData["SuccessMessage"] = "Registration successful! Please login to continue.";
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    if (Request.Headers["Content-Type"].ToString().Contains("application/json"))
                    {
                        return Json(new { success = false, message = result.Message });
                    }
                    ModelState.AddModelError("", result.Message);
                }
            }
            catch (Exception ex)
            {
                if (Request.Headers["Content-Type"].ToString().Contains("application/json"))
                {
                    return Json(new { success = false, message = $"An error occurred: {ex.Message}" });
                }
                ModelState.AddModelError("", $"An error occurred: {ex.Message}");
            }

            return Json(new { success = false, message = "Registration failed" });
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            try
            {
                await _apiService.PostAsync("https://localhost:7049/api/v1/Auth/Logout"); // (nếu có endpoint)
            }
            catch
            {
                // Log error nhưng vẫn tiếp tục logout
            }

            // Xoá token trong Session
            HttpContext.Session.Remove("JWToken");
            // Cookie authentication đã vô hiệu hoá

            // Nếu là AJAX request, trả về JSON để frontend xử lý
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new { success = true, redirectUrl = "/", clearToken = true });
            }

            return RedirectToAction("Index", "Home");
        }
    }
}
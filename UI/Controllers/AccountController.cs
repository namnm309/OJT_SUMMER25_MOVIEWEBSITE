using Microsoft.AspNetCore.Mvc;
using UI.Models;
using UI.Areas.UserManagement.Models;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using UI.Services;

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
                var loginData = new
                {
                    account = model.Username,  // AuthController sử dụng 'account' thay vì 'username'
                    password = model.Password
                };

                // Thay đổi endpoint từ /api/user/login sang /api/v1/Auth/login
                var result = await _apiService.PostAsync<JsonElement>("/api/v1/Auth/login", loginData);

                if (result.Success && result.Data.ValueKind != JsonValueKind.Undefined)
                {
                    var responseData = result.Data;

                    // Kiểm tra xem có property "data" không
                    if (responseData.TryGetProperty("data", out var dataElement))
                    {
                        responseData = dataElement; // Sử dụng data bên trong
                    }

                    // Lấy token từ response
                    string token = "";
                    if (responseData.TryGetProperty("token", out var tokenProp))
                    {
                        token = tokenProp.GetString() ?? "";
                    }

                    // Lấy thông tin user từ response
                    string email = "";
                    string fullName = "";
                    string role = "Customer";
                    string userId = ""; // ✅ Thêm biến userId

                    if (responseData.TryGetProperty("email", out var emailProp))
                    {
                        email = emailProp.GetString() ?? "";
                    }
                    if (responseData.TryGetProperty("fullName", out var fullNameProp))
                    {
                        fullName = fullNameProp.GetString() ?? model.Username;
                    }
                    if (responseData.TryGetProperty("role", out var roleProp))
                    {
                        role = roleProp.GetString() ?? "Customer";
                    }
                    // ✅ Lấy UserId từ API response
                    if (responseData.TryGetProperty("userId", out var userIdProp))
                    {
                        userId = userIdProp.GetString() ?? Guid.NewGuid().ToString();
                    }
                    else
                    {
                        userId = Guid.NewGuid().ToString(); // Fallback nếu API không trả về
                    }

                    // Tạo claims cho cookie authentication
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.NameIdentifier, userId), // ✅ Sử dụng UserId thực
                        new Claim("UserId", userId), // ✅ Thêm claim UserId riêng
                        new Claim(ClaimTypes.Name, model.Username),
                        new Claim(ClaimTypes.Role, role),
                        new Claim("FullName", fullName),
                        new Claim("Email", email), // ✅ Thêm email vào claims
                        new Claim("Token", token)
                    };

                    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    var authProperties = new AuthenticationProperties
                    {
                        IsPersistent = model.RememberMe,
                        ExpiresUtc = model.RememberMe
                            ? DateTimeOffset.UtcNow.AddDays(7)
                            : DateTimeOffset.UtcNow.AddMinutes(30)
                    };

                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                        new ClaimsPrincipal(claimsIdentity), authProperties);

                    // Trả về response với token cho frontend
                    return new JsonResult(new
                    {
                        success = true,
                        role = role.ToLower(),
                        redirectUrl = "/",
                        userId = userId, // ✅ Sử dụng UserId thực từ API
                        username = model.Username,
                        fullName = fullName,
                        token = token
                    });
                }
                else
                {
                    var message = "Đăng nhập không thành công.";
                    if (result.Data.TryGetProperty("message", out var messageProp))
                    {
                        message = messageProp.GetString() ?? message;
                    }
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
                await _apiService.PostAsync("/api/user/logout");
            }
            catch
            {
                // Log error nhưng vẫn tiếp tục logout
            }

            // Sign out từ UI authentication
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            // Nếu là AJAX request, trả về JSON để frontend xử lý
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new { success = true, redirectUrl = "/", clearToken = true });
            }

            return RedirectToAction("Index", "Home");
        }
    }
}
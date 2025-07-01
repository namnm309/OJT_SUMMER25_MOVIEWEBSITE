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
                    username = model.Username,
                    password = model.Password,
                    rememberMe = model.RememberMe
                };

                var result = await _apiService.PostAsync<JsonElement>("/api/user/login", loginData);

                if (result.Success && result.Data.ValueKind != JsonValueKind.Undefined)
                {
                    var userElement = result.Data;
                    
                    // Kiểm tra xem có thuộc tính user không (dựa trên phản hồi API thực tế)
                    if (!userElement.TryGetProperty("user", out var userData))
                    {
                        if (Request.Headers["Content-Type"].ToString().Contains("application/json"))
                        {
                            return Json(new { success = false, message = "Invalid response format: missing user data" });
                        }
                        ModelState.AddModelError("", "Invalid response format: missing user data");
                        return Json(new { success = false, message = "Invalid response format: missing user data" });
                    }
                    
                    // Kiểm tra các thuộc tính cần thiết trong user object
                    if (!userData.TryGetProperty("userId", out var userIdProp))
                    {
                        if (Request.Headers["Content-Type"].ToString().Contains("application/json"))
                        {
                            return Json(new { success = false, message = "Invalid response format: missing userId" });
                        }
                        ModelState.AddModelError("", "Invalid response format: missing userId");
                        return Json(new { success = false, message = "Invalid response format: missing userId" });
                    }
                    
                    if (!userData.TryGetProperty("username", out var usernameProp))
                    {
                        if (Request.Headers["Content-Type"].ToString().Contains("application/json"))
                        {
                            return Json(new { success = false, message = "Invalid response format: missing username" });
                        }
                        ModelState.AddModelError("", "Invalid response format: missing username");
                        return Json(new { success = false, message = "Invalid response format: missing username" });
                    }
                    
                    if (!userData.TryGetProperty("role", out var roleProp))
                    {
                        if (Request.Headers["Content-Type"].ToString().Contains("application/json"))
                        {
                            return Json(new { success = false, message = "Invalid response format: missing role" });
                        }
                        ModelState.AddModelError("", "Invalid response format: missing role");
                        return Json(new { success = false, message = "Invalid response format: missing role" });
                    }
                    
                    // Kiểm tra fullName, sử dụng giá trị mặc định nếu không có
                    string fullName = userData.TryGetProperty("fullName", out var fullNameProp) 
                        ? fullNameProp.GetString() ?? model.Username 
                        : model.Username;
                    
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.NameIdentifier, userIdProp.ToString()),
                        new Claim(ClaimTypes.Name, usernameProp.GetString()!),
                        new Claim(ClaimTypes.Role, roleProp.ToString()),
                        new Claim("FullName", fullName)
                    };

                    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    var authProperties = new AuthenticationProperties
                    {
                        IsPersistent = model.RememberMe,
                        ExpiresUtc = model.RememberMe 
                            ? DateTimeOffset.UtcNow.AddDays(7)  // 7 ngày cho tùy chọn "Ghi nhớ"
                            : DateTimeOffset.UtcNow.AddMinutes(30)  // 30 phút cho phiên đăng nhập thường
                    };

                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), authProperties);
                    
                    // Xử lý chuyển hướng dựa trên vai trò người dùng
                    string role = roleProp.ToString().ToLower();
                    
                    // Nếu là AJAX request
                    if (Request.Headers["Content-Type"].ToString().Contains("application/json"))
                    {
                        return Json(new { success = true, role = role });
                    }
                    
                    // Redirect dựa trên returnUrl hoặc vai trò
                    if (!string.IsNullOrEmpty(model.ReturnUrl))
                    {
                        return Redirect(model.ReturnUrl);
                    }
                    else if (role == "admin")
                    {
                        return RedirectToAction("Index", "Dashboard");
                    }
                    else
                    {
                        return RedirectToAction("Index", "Home");
                    }
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

            return Json(new { success = false, message = "Login failed" });
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
                // Chuyển đổi giới tính từ chuỗi sang số (1=Nam, 2=Nữ)
                int genderValue = 1; // Mặc định là Nam
                if (!string.IsNullOrEmpty(model.Gender))
                {
                    if (model.Gender.Equals("Nữ", StringComparison.OrdinalIgnoreCase))
                    {
                        genderValue = 2; // Nữ
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
                    gender = genderValue // Giá trị số tương ứng với enum UserGender
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
                // Ghi log lỗi nhưng vẫn tiếp tục đăng xuất
            }

            // Đăng xuất khỏi hệ thống xác thực UI
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            return RedirectToAction("Index", "Home");
        }
    }
}
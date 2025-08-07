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
using Microsoft.Extensions.Logging; // Added for logging

namespace UI.Controllers
{
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
                // Lấy tất cả lỗi validation
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                var errorMessage = errors.Any() ? string.Join(", ", errors) : "Vui lòng nhập đầy đủ thông tin";
                
                return Json(new { success = false, message = errorMessage });
            }

            try
            {
                // Gửi tới API mới dùng JWT
                var loginData = new
                {
                    account = model.Username,
                    password = model.Password
                };

                var result = await _apiService.PostAsync<JsonElement>("api/v1/Auth/Login", loginData);

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
                    if (!string.IsNullOrWhiteSpace(model.ReturnUrl))
                    {
                        redirectUrl = model.ReturnUrl;
                    }
                    else
                    {
                        // Tự động redirect theo role
                        if (role.ToLower() == "admin" || role == "2")
                            redirectUrl = "/Dashboard/AdminDashboard";
                        else if (role.ToLower() == "staff" || role == "3")
                            redirectUrl = "/Dashboard/StaffDashboard";
                        else
                            redirectUrl = "/";
                    }

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
                    return new JsonResult(new { success = false, message = "Sai tên tài khoản hoặc mật khẩu" });
                }
            }
            catch (Exception ex)
            {
                if (Request.Headers["Content-Type"].ToString().Contains("application/json"))
                {
                    return Json(new { success = false, message = "Sai tên tài khoản hoặc mật khẩu" });
                }
                ModelState.AddModelError("", "Sai tên tài khoản hoặc mật khẩu");
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
                // Kiểm tra nếu là AJAX request (từ JavaScript)
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest" || 
                    Request.Headers["Content-Type"].ToString().Contains("application/x-www-form-urlencoded"))
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                    return Json(new { success = false, message = string.Join(", ", errors) });
                }
                return View(model);
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
                    if (Request.Headers["X-Requested-With"] == "XMLHttpRequest" || 
                        Request.Headers["Content-Type"].ToString().Contains("application/x-www-form-urlencoded"))
                    {
                        return Json(new { success = true, message = "Registration successful! Please login to continue." });
                    }

                    TempData["SuccessMessage"] = "Registration successful! Please login to continue.";
                    return RedirectToAction("Login", "Account");
                }
                else
                {
                    if (Request.Headers["X-Requested-With"] == "XMLHttpRequest" || 
                        Request.Headers["Content-Type"].ToString().Contains("application/x-www-form-urlencoded"))
                    {
                        return Json(new { success = false, message = result.Message });
                    }
                    ModelState.AddModelError("", result.Message);
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest" || 
                    Request.Headers["Content-Type"].ToString().Contains("application/x-www-form-urlencoded"))
                {
                    return Json(new { success = false, message = $"An error occurred: {ex.Message}" });
                }
                ModelState.AddModelError("", $"An error occurred: {ex.Message}");
            }

            // Nếu không phải AJAX request, trả về view với model
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest" || 
                Request.Headers["Content-Type"].ToString().Contains("application/x-www-form-urlencoded"))
            {
                return Json(new { success = false, message = "Registration failed" });
            }
            return View(model);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            try
            {
                await _apiService.PostAsync("https://cinemacity-backend-hhasbzggfafpgbgw.eastasia-01.azurewebsites.net/api/v1/Auth/Logout"); // (nếu có endpoint)
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

        [HttpGet]
        public IActionResult ForgotPassword()
        {
            // Pass TempData message to view if exists
            if (TempData["SuccessMessage"] != null)
            {
                ViewBag.SuccessMessage = TempData["SuccessMessage"].ToString();
            }
            return View(new ForgotPasswordViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                    return Json(new { success = false, message = string.Join(", ", errors) });
                }
                return View(model);
            }

            try
            {
                var requestData = new
                {
                    Email = model.Email
                };

                var result = await _apiService.PostAsync<JsonElement>("api/v1/Auth/Forgot-Password", requestData);

                if (result.Success)
                {
                    if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    {
                        return Json(new { success = true, message = "Mã OTP đã được gửi đến email của bạn." });
                    }

                    TempData["SuccessMessage"] = "Mã OTP đã được gửi đến email của bạn.";
                    TempData["Email"] = model.Email;
                    return RedirectToAction("ResetPassword");
                }
                else
                {
                    if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    {
                        return Json(new { success = false, message = result.Message });
                    }
                    ModelState.AddModelError("", result.Message);
                }
            }
            catch (Exception ex)
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = false, message = $"Đã xảy ra lỗi: {ex.Message}" });
                }
                ModelState.AddModelError("", $"Đã xảy ra lỗi: {ex.Message}");
            }

            return View(model);
        }

        [HttpGet]
        public IActionResult ResetPassword(string? email = null)
        {
            // Try to get email from query parameter first, then from TempData
            var emailValue = email ?? TempData["Email"]?.ToString();
            

            
            if (string.IsNullOrEmpty(emailValue))
            {
                return RedirectToAction("ForgotPassword");
            }

            var model = new ResetPasswordViewModel
            {
                Email = emailValue
            };

            // Pass TempData message to view if exists
            if (TempData["SuccessMessage"] != null)
            {
                ViewBag.SuccessMessage = TempData["SuccessMessage"].ToString();
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                var errorMessage = string.Join(", ", errors);
                
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = false, message = errorMessage });
                }
                
                ModelState.AddModelError("", errorMessage);
                return View(model);
            }

            try
            {
                // Additional validation before sending request
                if (string.IsNullOrEmpty(model.Email) || string.IsNullOrEmpty(model.OTP) || string.IsNullOrEmpty(model.NewPassword))
                {
                    var errorMessage = "Vui lòng điền đầy đủ thông tin bắt buộc";
                    
                    if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    {
                        return Json(new { success = false, message = errorMessage });
                    }
                    ModelState.AddModelError("", errorMessage);
                    return View(model);
                }

                var requestData = new
                {
                    email = model.Email.Trim(),
                    otp = model.OTP.Trim(),
                    newPassword = model.NewPassword
                };

                var result = await _apiService.PostAsync<JsonElement>("api/v1/Auth/Verify-ChangePassword", requestData);

                if (result.Success)
                {
                    if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    {
                        return Json(new { success = true, message = "Đặt lại mật khẩu thành công! Vui lòng đăng nhập với mật khẩu mới.", redirectUrl = "/Account/Login" });
                    }

                    TempData["SuccessMessage"] = "Đặt lại mật khẩu thành công! Vui lòng đăng nhập với mật khẩu mới.";
                    return RedirectToAction("Login");
                }
                else
                {
                    var errorMessage = result.Message ?? "Đã xảy ra lỗi khi đặt lại mật khẩu";
                    
                    if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    {
                        return Json(new { success = false, message = errorMessage });
                    }
                    ModelState.AddModelError("", errorMessage);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in ResetPassword action");
                var errorMessage = "Đã xảy ra lỗi khi kết nối đến server. Vui lòng thử lại.";
                
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = false, message = errorMessage });
                }
                ModelState.AddModelError("", errorMessage);
            }

            return View(model);
        }

        // Remote validation methods
        [HttpGet]
        public async Task<IActionResult> CheckPhoneUnique(string phone)
        {
            try
            {
                if (string.IsNullOrEmpty(phone))
                    return Json(true);

                var response = await _apiService.GetAsync<JsonElement>($"/api/user/check-phone-exists?phone={phone}");
                
                if (response.Success)
                {
                    var exists = response.Data.GetProperty("exists").GetBoolean();
                    return Json(!exists); // Return false if phone exists (validation fails)
                }
                
                return Json(true); // Default to valid if can't check
            }
            catch
            {
                return Json(true); // Default to valid on error
            }
        }

        [HttpGet]
        public async Task<IActionResult> CheckUsernameUnique(string username)
        {
            try
            {
                if (string.IsNullOrEmpty(username))
                    return Json(true);

                var response = await _apiService.GetAsync<JsonElement>($"/api/user/check-username-exists?username={username}");
                
                if (response.Success)
                {
                    var exists = response.Data.GetProperty("exists").GetBoolean();
                    return Json(!exists); // Return false if username exists (validation fails)
                }
                
                return Json(true); // Default to valid if can't check
            }
            catch
            {
                return Json(true); // Default to valid on error
            }
        }

        [HttpGet]
        public async Task<IActionResult> CheckEmailUnique(string email)
        {
            try
            {
                if (string.IsNullOrEmpty(email))
                    return Json(true);

                var response = await _apiService.GetAsync<JsonElement>($"/api/user/check-email-exists?email={email}");
                
                if (response.Success)
                {
                    var exists = response.Data.GetProperty("exists").GetBoolean();
                    return Json(!exists); // Return false if email exists (validation fails)
                }
                
                return Json(true); // Default to valid if can't check
            }
            catch
            {
                return Json(true); // Default to valid on error
            }
        }

        [HttpGet]
        public async Task<IActionResult> CheckIdentityCardUnique(string identityCard)
        {
            try
            {
                if (string.IsNullOrEmpty(identityCard))
                    return Json(true);

                var response = await _apiService.GetAsync<JsonElement>($"/api/user/check-identity-card-exists?identityCard={identityCard}");
                
                if (response.Success)
                {
                    var exists = response.Data.GetProperty("exists").GetBoolean();
                    return Json(!exists); // Return false if identity card exists (validation fails)
                }
                
                return Json(true); // Default to valid if can't check
            }
            catch
            {
                return Json(true); // Default to valid on error
            }
        }
    }
}
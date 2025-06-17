using Microsoft.AspNetCore.Mvc;
using UI.Models;
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
                            ? DateTimeOffset.UtcNow.AddDays(7)  // 7 days for "Remember me"
                            : DateTimeOffset.UtcNow.AddMinutes(30)  // 30 minutes for regular session
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

            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        [Authorize]
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
                TempData["ErrorMessage"] = $"Error loading profile: {ex.Message}";
            }

            return RedirectToAction("Index", "Dashboard");
        }

        [HttpPost]
        [Authorize]
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
                ModelState.AddModelError("", $"An error occurred: {ex.Message}");
            }

            return View(model);
        }
    }
}
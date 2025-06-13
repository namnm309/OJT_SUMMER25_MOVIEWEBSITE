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

        [HttpGet]
        public async Task<IActionResult> Login(string? returnUrl = null, bool forceLogout = false)
        {
            // Option để force logout trước khi show login page
            if (forceLogout && User.Identity?.IsAuthenticated == true)
            {
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                return RedirectToAction("Login", new { returnUrl });
            }
            
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Dashboard");
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
                return View(model);
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
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.NameIdentifier, userElement.GetProperty("userId").ToString()),
                        new Claim(ClaimTypes.Name, userElement.GetProperty("username").GetString()!),
                        new Claim(ClaimTypes.Role, userElement.GetProperty("role").ToString()),
                        new Claim("FullName", userElement.GetProperty("fullName").GetString()!)
                    };

                    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    var authProperties = new AuthenticationProperties
                    {
                        IsPersistent = false,  // Disable persistence for debugging
                        ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(30)  // Short session for testing
                    };

                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), authProperties);
                    
                    // Redirect based on return URL or role
                    if (!string.IsNullOrEmpty(model.ReturnUrl))
                    {
                        return Redirect(model.ReturnUrl);
                    }

                    return RedirectToAction("Index", "Dashboard");
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

        [HttpGet]
        public IActionResult Register()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Dashboard");
            }

            return View(new RegisterViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
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
                    gender = model.Gender
                };

                var result = await _apiService.PostAsync<JsonElement>("/api/user/register", registerData);

                if (result.Success)
                {
                    TempData["SuccessMessage"] = "Registration successful! Please login to continue.";
                    return RedirectToAction("Login");
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

            return RedirectToAction("Login");
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
                        UserId = Guid.Parse(userProfile.GetProperty("userId").ToString()),
                        Username = userProfile.GetProperty("username").GetString()!,
                        Email = userProfile.GetProperty("email").GetString()!,
                        FullName = userProfile.GetProperty("fullName").GetString()!,
                        Phone = userProfile.GetProperty("phone").GetString()!,
                        IdentityCard = userProfile.GetProperty("identityCard").GetString()!,
                        Address = userProfile.GetProperty("address").GetString()!,
                        Score = userProfile.GetProperty("score").GetDouble(),
                        Role = userProfile.GetProperty("role").ToString(),
                        BirthDate = userProfile.TryGetProperty("birthDate", out var bd) && !bd.ValueKind.Equals(JsonValueKind.Null) 
                            ? DateTime.Parse(bd.GetString()!) : null,
                        Gender = userProfile.TryGetProperty("gender", out var g) && !g.ValueKind.Equals(JsonValueKind.Null) 
                            ? g.GetString() : null,
                        Avatar = userProfile.TryGetProperty("avatar", out var a) && !a.ValueKind.Equals(JsonValueKind.Null) 
                            ? a.GetString() : null
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
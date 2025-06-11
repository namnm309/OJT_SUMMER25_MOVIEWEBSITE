using Microsoft.AspNetCore.Mvc;
using UI.Models;
using System.Text.Json;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace UI.Controllers
{
    public class AccountController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiBaseUrl;

        public AccountController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClient = httpClientFactory.CreateClient("ApiClient");
            _apiBaseUrl = configuration["ApiSettings:BaseUrl"] ?? "https://localhost:7049"; // API URL
        }

        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
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

                var json = JsonSerializer.Serialize(loginData);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync($"{_apiBaseUrl}/api/user/login", content);
                var responseContent = await response.Content.ReadAsStringAsync();

                // Debug logging
                Console.WriteLine($"🔍 API Response Status: {response.StatusCode}");
                Console.WriteLine($"🔍 API Response Content: {responseContent}");

                if (response.IsSuccessStatusCode)
                {
                    var result = JsonSerializer.Deserialize<JsonElement>(responseContent);
                    
                    // Check if login was actually successful
                    if (result.TryGetProperty("success", out var successProp) && !successProp.GetBoolean())
                    {
                        var message = result.TryGetProperty("message", out var messageProp) 
                            ? messageProp.GetString() 
                            : "Login failed";
                        ModelState.AddModelError("", message ?? "Login failed");
                        return View(model);
                    }
                    
                    // Forward cookies từ API response tới UI
                    if (response.Headers.TryGetValues("Set-Cookie", out var cookies))
                    {
                        foreach (var cookie in cookies)
                        {
                            Response.Headers.Add("Set-Cookie", cookie);
                        }
                    }
                    
                    // Parse user data và tạo claims cho UI
                    var userElement = result.GetProperty("user");
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
                        IsPersistent = model.RememberMe,
                        ExpiresUtc = model.RememberMe ? DateTimeOffset.UtcNow.AddDays(7) : DateTimeOffset.UtcNow.AddHours(2)
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
                    var errorResult = JsonSerializer.Deserialize<JsonElement>(responseContent);
                    var message = errorResult.TryGetProperty("message", out var messageProp) 
                        ? messageProp.GetString() 
                        : "Login failed";
                    
                    ModelState.AddModelError("", message ?? "Login failed");
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

                var json = JsonSerializer.Serialize(registerData);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync($"{_apiBaseUrl}/api/user/register", content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = "Registration successful! Please login to continue.";
                    return RedirectToAction("Login");
                }
                else
                {
                    var errorResult = JsonSerializer.Deserialize<JsonElement>(responseContent);
                    var message = errorResult.TryGetProperty("message", out var messageProp) 
                        ? messageProp.GetString() 
                        : "Registration failed";
                    
                    ModelState.AddModelError("", message ?? "Registration failed");
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
                await _httpClient.PostAsync($"{_apiBaseUrl}/api/user/logout", null);
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
                var response = await _httpClient.GetAsync($"{_apiBaseUrl}/api/user/profile");
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var userProfile = JsonSerializer.Deserialize<JsonElement>(responseContent);

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

                var json = JsonSerializer.Serialize(editData);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PutAsync($"{_apiBaseUrl}/api/user/profile", content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = "Profile updated successfully!";
                    return RedirectToAction("Profile");
                }
                else
                {
                    var errorResult = JsonSerializer.Deserialize<JsonElement>(responseContent);
                    var message = errorResult.TryGetProperty("message", out var messageProp) 
                        ? messageProp.GetString() 
                        : "Update failed";
                    
                    ModelState.AddModelError("", message ?? "Update failed");
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
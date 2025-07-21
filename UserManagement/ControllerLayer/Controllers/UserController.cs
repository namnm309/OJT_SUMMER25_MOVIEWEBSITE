using ApplicationLayer.DTO.UserManagement;
using ApplicationLayer.Services.UserManagement;
using DomainLayer.Enum;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text;
using System.Globalization;

namespace ControllerLayer.Controllers
{
    [ApiController]
    [Route("api/v1/user")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto loginRequest)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                return BadRequest(new { message = "Validation failed", errors = errors });
            }

            var result = await _userService.LoginAsync(loginRequest);
            if (!result.Success)
            {
                return Ok(new { success = false, message = result.Message });
            }

            // Create session ID
            var sessionId = Guid.NewGuid().ToString();

            // Create claims for cookie
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, result.User!.UserId.ToString()),
                new Claim(ClaimTypes.Name, result.User.Username),
                new Claim(ClaimTypes.Role, result.User.Role.ToString()),
                new Claim("SessionId", sessionId),
                new Claim("FullName", result.User.FullName)
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = loginRequest.RememberMe,
                ExpiresUtc = loginRequest.RememberMe ? DateTimeOffset.UtcNow.AddDays(7) : DateTimeOffset.UtcNow.AddHours(2)
            };

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, 
                new ClaimsPrincipal(claimsIdentity), authProperties);

            return Ok(new
            {
                success = true,
                message = result.Message,
                user = result.User,
                redirectUrl = GetRedirectUrl(result.User.Role)
            });
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequestDto registerRequest)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _userService.RegisterAsync(registerRequest);
            if (!result.Success)
            {
                return Ok(new { success = false, message = result.Message });
            }

            return Ok(new { success = true, message = result.Message });
        }

        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return Ok(new { message = "Logout successful" });
        }

        [HttpGet("profile")]
        [Authorize]
        public async Task<IActionResult> GetProfile()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out Guid userId))
            {
                return Unauthorized();
            }

            var user = await _userService.GetUserByIdAsync(userId);
            if (user == null)
            {
                return NotFound(new { message = "User not found" });
            }

            return Ok(user);
        }

        [HttpPut("profile")]
        [AllowAnonymous]
        public async Task<IActionResult> EditProfile([FromBody] EditProfileRequestDto editRequest)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out Guid userId))
            {

                return Unauthorized();
            }

            var result = await _userService.EditProfileAsync(userId, editRequest);
            if (!result.Success)
            {
                return BadRequest(new { message = result.Message });
            }

            return Ok(new
            {
                message = result.Message,
                user = result.User
            });
        }

        [HttpGet("members")]
        [AllowAnonymous]
        public async Task<IActionResult> GetAllMembers(string? search = null)
        {
            var members = await _userService.GetAllMembersAsync();
            if (!string.IsNullOrEmpty(search))
            {
                var normalizedSearch = NormalizeString(search);
                members = members.Where(m => NormalizeString(m.FullName).Contains(normalizedSearch, StringComparison.OrdinalIgnoreCase)).ToList();
            }
            return Ok(new { success = true, data = members });
        }

        private string NormalizeString(string input)
        {
            var normalized = input.Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder();
            foreach (var c in normalized)
            {
                var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                {
                    sb.Append(c);
                }
            }
            return sb.ToString().Normalize(NormalizationForm.FormC);
        }

        // ============ ADMIN OPERATIONS ============

        [HttpPost]
        //[Authorize(Roles = "Admin")]
        [AllowAnonymous]
        public async Task<IActionResult> CreateUser([FromBody] UserCreateDto createRequest)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors)
                                              .Select(e => string.IsNullOrEmpty(e.ErrorMessage) ? "Invalid value" : e.ErrorMessage)
                                              .ToList();
                return BadRequest(new {
                    success = false,
                    message = string.Join("; ", errors)
                });
            }

            var result = await _userService.CreateUserAsync(createRequest);
            if (!result.Success)
            {
                return BadRequest(new { success = false, message = result.Message });
            }

            return Ok(new 
            { 
                success = true, 
                message = result.Message, 
                data = result.User 
            });
        }

        [HttpPatch("{id}")]
        //[Authorize(Roles = "Admin")]
        [AllowAnonymous]
        public async Task<IActionResult> UpdateUser(Guid id, [FromBody] UserUpdateDto updateRequest)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _userService.UpdateUserAsync(id, updateRequest);
            if (!result.Success)
            {
                return BadRequest(new { success = false, message = result.Message });
            }

            return Ok(new 
            { 
                success = true, 
                message = result.Message, 
                data = result.User 
            });
        }

        [HttpDelete("{id}")]
        //[Authorize(Roles = "Admin")]
        [AllowAnonymous]
        public async Task<IActionResult> DeleteUser(Guid id)
        {
            var result = await _userService.DeleteUserAsync(id);
            if (!result.Success)
            {
                return BadRequest(new { success = false, message = result.Message });
            }

            return Ok(new { success = true, message = result.Message });
        }

        [HttpPatch("{id}/status")]
        //[Authorize(Roles = "Admin")]
        [AllowAnonymous]
        public async Task<IActionResult> ToggleUserStatus(Guid id)
        {
            var result = await _userService.ToggleUserStatusAsync(id);
            if (!result.Success)
            {
                return BadRequest(new { success = false, message = result.Message });
            }

            return Ok(new 
            { 
                success = true, 
                message = result.Message, 
                data = result.User 
            });
        }

        [HttpGet("{id}")]
        //[Authorize(Roles = "Admin,Staff")]
        [AllowAnonymous]
        public async Task<IActionResult> GetUserById(Guid id)
        {
            var user = await _userService.GetUserByIdAsync(id);
            if (user == null)
            {
                return NotFound(new { success = false, message = "User not found" });
            }

            return Ok(new { success = true, data = user });
        }

        private string GetRedirectUrl(UserRole role)
        {
            return role switch
            {
                UserRole.Admin => "/Dashboard/AdminDashboard",
                UserRole.Staff => "/Dashboard/StaffDashboard", 
                UserRole.Member => "/Dashboard/MemberDashboard",
                _ => "/Home/Index"
            };
        }
    }
}
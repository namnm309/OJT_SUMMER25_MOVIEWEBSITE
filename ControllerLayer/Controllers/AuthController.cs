using ApplicationLayer.DTO.JWT;
using ApplicationLayer.Middlewares;
using ApplicationLayer.Services.JWT;
using Microsoft.AspNetCore.Mvc;

namespace ControllerLayer.Controllers
{
    [ApiController]
    [Route("api/v1/Auth")]
    public class AuthController : Controller
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IAuthService authService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        [HttpPost("Register")]
        public async Task<IActionResult> Register([FromBody] RegisterReq req)
        {
            _logger.LogInformation("Register");

            return await _authService.HandleRegister(req);
        }

        [HttpPost("Login")]
        public async Task<IActionResult> Login([FromBody] LoginReq req)
        {
            _logger.LogInformation("Login");

            return await _authService.HandleLogin(req);
        }

        [Protected]
        [HttpGet("View")]
        public new async Task<IActionResult> View()
        {
            _logger.LogInformation("View");
            return await _authService.ViewUser();
        }

        [Protected]
        [HttpPatch("Edit")]
        public async Task<IActionResult> Edit(EditUserReq req)
        {
            _logger.LogInformation("Edit Profile");

            return await _authService.HandleEditProfile(req);
        }

        [HttpPost("Register-Email")]
        public async Task<IActionResult> RegisterForCustomer([FromBody] RegisterReq req)
        {
            _logger.LogInformation("Register For Customer");

            return await _authService.HandleRegisterForCustomer(req);
        }

        [HttpPost("Forgot-Password")]
        public async Task<IActionResult> ForgotPasword([FromBody] RequestOTP req)
        {
            _logger.LogInformation("Forgot Pasword");

            return await _authService.ForgotPassword(req);
        }

        [HttpPost("Verify-ChangePassword")]
        public async Task<IActionResult> VerifyChangePassword([FromBody] VerifyOTPChangePassword req)
        {
            _logger.LogInformation("Verify Change Pasword");

            return await _authService.HandleVerifyOTPChangePassword(req);
        }
    }
}

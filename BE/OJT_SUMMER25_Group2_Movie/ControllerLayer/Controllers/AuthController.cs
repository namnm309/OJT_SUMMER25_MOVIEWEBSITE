using ApplicationLayer.DTO.Auth;
using ApplicationLayer.Services;
using Microsoft.AspNetCore.Mvc;

namespace ControllerLayer.Controllers
{
    [Route("auth")]
    public class AuthController : Controller
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        // GET: /auth/register
        [HttpGet("register")]
        public IActionResult Register()
        {
            return View();
        }

        // POST: /auth/register
        [HttpPost("register")]
        public async Task<IActionResult> Register(Register model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var result = await _authService.HandleRegister(model);

            if (result is BadRequestObjectResult badRequest)
            {
                ModelState.AddModelError(string.Empty, badRequest.Value?.ToString());
                return View(model);
            }

            // Nếu thành công, chuyển về trang "Register thành công" hoặc login
            return RedirectToAction("RegisterSuccess");
        }

        // Optional: trang thông báo đăng ký thành công
        [HttpGet("register-success")]
        public IActionResult RegisterSuccess()
        {
            return View(); // tạo View tên RegisterSuccess.cshtml nếu cần
        }
    }
}

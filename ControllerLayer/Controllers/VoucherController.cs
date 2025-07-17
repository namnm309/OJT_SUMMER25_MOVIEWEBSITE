using ApplicationLayer.Services.PromotionManagement;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ControllerLayer.Controllers
{
    [ApiController]
    [Route("api/v1/vouchers")]
    public class VoucherController : Controller
    {
        private readonly IUserPromotionService _userPromotionService;
        private readonly ILogger<VoucherController> _logger;

        public VoucherController(IUserPromotionService userPromotionService, ILogger<VoucherController> logger)
        {
            _userPromotionService = userPromotionService;
            _logger = logger;
        }

        /// <summary>
        /// Get vouchers of currently authenticated user
        /// </summary>
        [HttpGet("my")]
        [Authorize]
        public async Task<IActionResult> GetMyVouchers()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out Guid userId))
            {
                return Unauthorized();
            }

            _logger.LogInformation("Fetching vouchers for user {UserId}", userId);
            return await _userPromotionService.GetUserVouchersAsync(userId);
        }

        /// <summary>
        /// Redeem voucher
        /// </summary>
        [HttpPost("redeem/{userPromotionId}")]
        [Authorize]
        public async Task<IActionResult> RedeemVoucher(Guid userPromotionId)
        {
            return await _userPromotionService.RedeemVoucherAsync(userPromotionId);
        }
    }
} 
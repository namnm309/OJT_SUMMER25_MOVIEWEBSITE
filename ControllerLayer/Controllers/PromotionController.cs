using ApplicationLayer.DTO.PromotionManagement;
using ApplicationLayer.Services.PromotionManagement;
using ApplicationLayer.Middlewares;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace ControllerLayer.Controllers
{
    [ApiController]
    [Route("api/v1/promotions")]
    public class PromotionController : Controller
    {
        private readonly IPromotionService _promotionService;
        private readonly ILogger<PromotionController> _logger;
        private readonly IUserPromotionService _userPromotionService;

        public PromotionController(
            IPromotionService promotionService,
            ILogger<PromotionController> logger,
            IUserPromotionService userPromotionService)
        {
            _promotionService = promotionService;
            _logger = logger;
            _userPromotionService = userPromotionService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllPromotions()
        {
            _logger.LogInformation("Getting all promotions");
            return await _promotionService.GetAllPromotions();
        }

        [HttpPost]
        public async Task<IActionResult> CreatePromotion([FromBody] PromotionCreateDto Dto)
        {
            _logger.LogInformation("Creating new promotion");
            return await _promotionService.CreatePromotion(Dto);
        }

        public class SaveUserPromotionRequest
        {
            public Guid PromotionId { get; set; }
        }
       

        [HttpGet("{id}")]
        public async Task<IActionResult> GetPromotionById(Guid id)
        {
            _logger.LogInformation($"Getting promotion with ID: {id}");
            return await _promotionService.GetPromotionById(id);
        }

        [HttpPut]
        public async Task<IActionResult> UpdatePromotion([FromBody] PromotionUpdateDto Dto)
        {
            _logger.LogInformation($"Updating promotion with ID: {Dto.Id}");
            return await _promotionService.UpdatePromotion(Dto);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePromotion(Guid id)
        {
            _logger.LogInformation($"Deleting promotion with ID: {id}");
            return await _promotionService.DeletePromotion(id);
        }

        /// <summary>
        /// lưu khuyến mãi cho người dùng
        /// </summary>
        [Protected]
        [HttpPost("save-user-promotion")]
        public async Task<IActionResult> SaveUserPromotion([FromBody] SaveUserPromotionRequest request)
        {
            _logger.LogInformation($"Saving promotion {request.PromotionId} for authenticated user");
            return await _promotionService.SaveUserPromotionAsync(request.PromotionId);
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
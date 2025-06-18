using ApplicationLayer.DTO.PromotionManagement;
using ApplicationLayer.Services.PromotionManagement;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace ControllerLayer.Controllers
{
    [ApiController]
    [Route("api/v1/promotions")]
    public class PromotionController : Controller
    {
        private readonly IPromotionService _promotionService;
        private readonly ILogger<PromotionController> _logger;

        public PromotionController(
            IPromotionService promotionService,
            ILogger<PromotionController> logger)
        {
            _promotionService = promotionService;
            _logger = logger;
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
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ApplicationLayer.DTO.PromotionManagement;
using Microsoft.AspNetCore.Mvc;

namespace ApplicationLayer.Services.PromotionManagement
{
    public interface IPromotionService
    {
        Task<IActionResult> CreatePromotion(PromotionCreateDto Dto);
        Task<IActionResult> GetAllPromotions();
        Task<IActionResult> GetPromotionById(Guid id);
        Task<IActionResult> UpdatePromotion(PromotionUpdateDto Dto);
        Task<IActionResult> DeletePromotion(Guid id);
        Task<IActionResult> SaveUserPromotionAsync(Guid userId, Guid promotionId);
    }
}

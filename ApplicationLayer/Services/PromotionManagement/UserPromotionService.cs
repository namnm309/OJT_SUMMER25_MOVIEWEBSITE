using System;
using System.Linq;
using System.Threading.Tasks;
using Application.ResponseCode;
using ApplicationLayer.DTO.PromotionManagement;
using AutoMapper;
using DomainLayer.Entities;
using InfrastructureLayer.Repository;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ApplicationLayer.Services.PromotionManagement
{
    public class UserPromotionService : IUserPromotionService
    {
        private readonly IGenericRepository<UserPromotion> _userPromotionRepo;
        private readonly IGenericRepository<Promotion> _promotionRepo;
        private readonly IMapper _mapper;

        public UserPromotionService(IGenericRepository<UserPromotion> userPromotionRepo,
            IGenericRepository<Promotion> promotionRepo,
            IMapper mapper)
        {
            _userPromotionRepo = userPromotionRepo;
            _promotionRepo = promotionRepo;
            _mapper = mapper;
        }

        public async Task<IActionResult> GetUserVouchersAsync(Guid userId)
        {
            var vouchers = await _userPromotionRepo.WhereAsync(up => up.UserId == userId, "Promotion");

            var result = vouchers.Select(v => new UserVoucherDto
            {
                UserPromotionId = v.Id,
                PromotionId = v.PromotionId,
                Title = v.Promotion.Title,
                DiscountPercent = v.Promotion.DiscountPercent,
                RequiredPoints = v.Promotion.RequiredPoints,
                Description = v.Promotion.Description,
                StartDate = v.Promotion.StartDate,
                EndDate = v.Promotion.EndDate,
                ImageUrl = v.Promotion.ImageUrl,
                IsRedeemed = v.IsRedeemed,
                RedeemedAt = v.RedeemedAt
            }).ToList();

            return SuccessResp.Ok(result);
        }

        public async Task<IActionResult> RedeemVoucherAsync(Guid userPromotionId)
        {
            var voucher = await _userPromotionRepo.FindByIdAsync(userPromotionId, "Promotion");
            if (voucher == null)
            {
                return ErrorResp.NotFound("Voucher not found");
            }

            if (voucher.IsRedeemed)
            {
                return ErrorResp.BadRequest("Voucher already redeemed");
            }

            voucher.IsRedeemed = true;
            voucher.RedeemedAt = DateTime.UtcNow;

            await _userPromotionRepo.UpdateAsync(voucher);

            return SuccessResp.Ok("Voucher redeemed successfully");
        }
    }
} 
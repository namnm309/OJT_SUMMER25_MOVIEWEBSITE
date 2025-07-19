using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.ResponseCode;
using ApplicationLayer.DTO.PromotionManagement;
using AutoMapper;
using DomainLayer.Entities;
using InfrastructureLayer.Repository;
using Microsoft.AspNetCore.Mvc;

namespace ApplicationLayer.Services.PromotionManagement
{
    public class PromotionService : IPromotionService
    {
        private readonly IGenericRepository<Promotion> _promotionRepo;
        private readonly IMapper _mapper;
        private readonly IGenericRepository<UserPromotion> _userPromotionRepo;
        private readonly IGenericRepository<Users> _userRepo;
        public PromotionService(IGenericRepository<Promotion> promotionRepo, IMapper mapper, IGenericRepository<UserPromotion> userPromotionRepo, IGenericRepository<Users> userRepo)
        {
            _promotionRepo = promotionRepo;
            _mapper = mapper;
            _userPromotionRepo = userPromotionRepo;
            _userRepo = userRepo;
        }

        public async Task<IActionResult> CreatePromotion(PromotionCreateDto Dto)
        {
            // Validate date range
            if (Dto.StartDate >= Dto.EndDate)
                return ErrorResp.BadRequest("Start date must be earlier than end date");

            // Check for duplicate title
            var existingPromotion = await _promotionRepo.FirstOrDefaultAsync(p =>
                p.Title.ToLower() == Dto.Title.ToLower());

            if (existingPromotion != null)
                return ErrorResp.BadRequest("A promotion with the same title already exists");

            var promotion = _mapper.Map<Promotion>(Dto);
            await _promotionRepo.CreateAsync(promotion);

            return SuccessResp.Created("Promotion created successfully");
        }

        public async Task<IActionResult> GetAllPromotions()
        {
            var promotions = await _promotionRepo.ListAsync();
            var result = _mapper.Map<List<PromotionResponseDto>>(promotions);
            return SuccessResp.Ok(result);
        }

        public async Task<IActionResult> GetPromotionById(Guid id)
        {
            var promotion = await _promotionRepo.FindByIdAsync(id);

            if (promotion == null)
                return ErrorResp.NotFound("Promotion not found");

            var result = _mapper.Map<PromotionResponseDto>(promotion);
            return SuccessResp.Ok(result);
        }

        public async Task<IActionResult> UpdatePromotion(PromotionUpdateDto Dto)
        {
            var promotion = await _promotionRepo.FindByIdAsync(Dto.Id);

            if (promotion == null)
                return ErrorResp.NotFound("Promotion not found");

            // Validate date range
            if (Dto.StartDate >= Dto.EndDate)
                return ErrorResp.BadRequest("Start date must be earlier than end date");

            // Check for duplicate title (excluding current promotion)
            var duplicateTitle = await _promotionRepo.FirstOrDefaultAsync(p =>
                p.Id != Dto.Id && p.Title.ToLower() == Dto.Title.ToLower());

            if (duplicateTitle != null)
                return ErrorResp.BadRequest("A promotion with the same title already exists");

            _mapper.Map(Dto, promotion);
            await _promotionRepo.UpdateAsync(promotion);

            return SuccessResp.Ok("Promotion updated successfully");
        }

        public async Task<IActionResult> DeletePromotion(Guid id)
        {
            var promotion = await _promotionRepo.FindByIdAsync(id);

            if (promotion == null)
                return ErrorResp.NotFound("Promotion not found");

            await _promotionRepo.DeleteAsync(promotion);
            return SuccessResp.Ok("Promotion deleted successfully");
        }

        public async Task<IActionResult> SaveUserPromotionAsync(Guid userId, Guid promotionId)
        {
            // Kiểm tra user tồn tại
            var user = await _userRepo.FindByIdAsync(userId);
            if (user == null)
                return ErrorResp.NotFound("User not found");

            // Kiểm tra promotion tồn tại
            var promotion = await _promotionRepo.FindByIdAsync(promotionId);
            if (promotion == null)
                return ErrorResp.NotFound("Promotion not found");

            // Kiểm tra đã lưu chưa
            var exist = await _userPromotionRepo.FirstOrDefaultAsync(up => up.UserId == userId && up.PromotionId == promotionId);
            if (exist != null)
                return ErrorResp.BadRequest("User already saved this promotion");

            // Lưu mới
            var userPromotion = new UserPromotion
            {
                UserId = userId,
                PromotionId = promotionId,
                IsRedeemed = false,
                RedeemedAt = null
            };
            await _userPromotionRepo.CreateAsync(userPromotion);
            return SuccessResp.Ok("Promotion saved for user successfully");
        }
    }
}

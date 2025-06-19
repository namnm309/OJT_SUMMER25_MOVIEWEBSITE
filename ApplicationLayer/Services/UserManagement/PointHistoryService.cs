using Application.ResponseCode;
using ApplicationLayer.DTO.PromotionManagement;
using AutoMapper;
using DomainLayer.Entities;
using InfrastructureLayer.Repository;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationLayer.Services.UserManagement
{
    public interface IPointHistoryService
    {
        Task<IActionResult> ViewPointHistory(Guid userId, PointHistoryFilterDto Dto);
    }
    public class PointHistoryService : IPointHistoryService
    {
        private readonly IGenericRepository<PointHistory> _pointHistoryRepo;
        private readonly IMapper _mapper;

        public PointHistoryService(IGenericRepository<PointHistory> pointHistoryRepo, IMapper mapper)
        {
            _pointHistoryRepo = pointHistoryRepo;
            _mapper = mapper;
        }

        public async Task<IActionResult> ViewPointHistory(Guid userId, PointHistoryFilterDto Dto)
        {
            var query = await _pointHistoryRepo.WhereAsync(
                filter: p => p.UserId == userId &&
                             (!Dto.IsUsed.HasValue || p.IsUsed == Dto.IsUsed) &&
                             (!Dto.FromDate.HasValue || p.CreatedAt >= Dto.FromDate.Value) &&
                             (!Dto.ToDate.HasValue || p.CreatedAt <= Dto.ToDate.Value),
                navigationProperties: new[] { "Booking", "Booking.ShowTime", "Booking.ShowTime.Movie" }
            );

            if (!query.Any())
                return SuccessResp.Ok("No score history found for the selected period.");

            var result = query.Select(ph => new PointHistoryDto
            {
                CreatedAt = ph.CreatedAt,
                MovieName = ph.Booking.ShowTime.Movie.Title,
                Points = ph.IsUsed ? -ph.Points : ph.Points
            }).ToList();

            return SuccessResp.Ok(result);
        }

    }
}

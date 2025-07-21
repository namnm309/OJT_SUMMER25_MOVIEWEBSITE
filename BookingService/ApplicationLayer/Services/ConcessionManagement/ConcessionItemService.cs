using ApplicationLayer.DTO.ConcessionManagement;
using AutoMapper;
using DomainLayer.Entities;
using InfrastructureLayer.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ApplicationLayer.Services.ConcessionManagement
{
    public class ConcessionItemService : IConcessionItemService
    {
        private readonly IConcessionItemRepository _repository;
        private readonly IMapper _mapper;

        public ConcessionItemService(IConcessionItemRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<ConcessionItemDto>> GetAllAsync()
        {
            var concessionItems = await _repository.GetAllAsync();
            return _mapper.Map<IEnumerable<ConcessionItemDto>>(concessionItems);
        }

        public async Task<ConcessionItemDto> GetByIdAsync(Guid id)
        {
            var concessionItem = await _repository.GetByIdAsync(id);
            if (concessionItem == null)
                return null;

            return _mapper.Map<ConcessionItemDto>(concessionItem);
        }

        public async Task<ConcessionItemDto> CreateAsync(CreateConcessionItemDto createDto)
        {
            var concessionItem = _mapper.Map<ConcessionItem>(createDto);
            concessionItem.CreatedAt = DateTime.UtcNow;
            concessionItem.UpdatedAt = DateTime.UtcNow;

            var createdItem = await _repository.CreateAsync(concessionItem);
            return _mapper.Map<ConcessionItemDto>(createdItem);
        }

        public async Task<ConcessionItemDto> UpdateAsync(Guid id, UpdateConcessionItemDto updateDto)
        {
            var existingItem = await _repository.GetByIdAsync(id);
            if (existingItem == null)
                throw new Exception($"ConcessionItem with ID {id} not found.");

            _mapper.Map(updateDto, existingItem);
            existingItem.UpdatedAt = DateTime.UtcNow;

            await _repository.UpdateAsync(existingItem);
            return _mapper.Map<ConcessionItemDto>(existingItem);
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var existingItem = await _repository.GetByIdAsync(id);
            if (existingItem == null)
                return false;

            await _repository.DeleteAsync(id);
            return true;
        }

        public async Task<IEnumerable<ConcessionItemDto>> GetActiveItemsAsync()
        {
            var activeItems = await _repository.GetActiveItemsAsync();
            return _mapper.Map<IEnumerable<ConcessionItemDto>>(activeItems);
        }

        public async Task<GetConcessionItemsResponse> GetPaginatedAsync(GetConcessionItemsRequest request)
        {
            // Lấy tất cả các items từ repository
            var allItems = await _repository.GetAllAsync();
            var query = allItems.AsQueryable();

            // Áp dụng bộ lọc
            if (request.IsActive.HasValue)
            {
                query = query.Where(x => x.IsActive == request.IsActive.Value);
            }

            if (!string.IsNullOrEmpty(request.SearchTerm))
            {
                query = query.Where(x =>
                    x.Name.Contains(request.SearchTerm, StringComparison.OrdinalIgnoreCase) ||
                    (x.Description != null && x.Description.Contains(request.SearchTerm, StringComparison.OrdinalIgnoreCase))
                );
            }

            // Đếm tổng số items
            var totalCount = query.Count();

            // Phân trang
            var pagedItems = query
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToList();

            // Mapping sang DTO
            var itemDtos = _mapper.Map<List<ConcessionItemDto>>(pagedItems);

            // Tạo response
            var response = new GetConcessionItemsResponse
            {
                Page = request.Page,
                PageSize = request.PageSize,
                Total = totalCount,
                Items = itemDtos
            };

            return response;
        }
    }
}

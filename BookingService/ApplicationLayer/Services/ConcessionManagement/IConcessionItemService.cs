using ApplicationLayer.DTO.ConcessionManagement;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ApplicationLayer.Services.ConcessionManagement
{
    public interface IConcessionItemService
    {
        Task<IEnumerable<ConcessionItemDto>> GetAllAsync();
        Task<ConcessionItemDto> GetByIdAsync(Guid id);
        Task<ConcessionItemDto> CreateAsync(CreateConcessionItemDto createDto);
        Task<ConcessionItemDto> UpdateAsync(Guid id, UpdateConcessionItemDto updateDto);
        Task<bool> DeleteAsync(Guid id);
        Task<IEnumerable<ConcessionItemDto>> GetActiveItemsAsync();
        Task<GetConcessionItemsResponse> GetPaginatedAsync(GetConcessionItemsRequest request);
    }
}

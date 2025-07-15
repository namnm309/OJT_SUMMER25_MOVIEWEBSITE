using DomainLayer.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace InfrastructureLayer.Repository
{
    public interface IConcessionItemRepository
    {
        Task<IEnumerable<ConcessionItem>> GetAllAsync();
        Task<ConcessionItem> GetByIdAsync(Guid id);
        Task<ConcessionItem> CreateAsync(ConcessionItem concessionItem);
        Task UpdateAsync(ConcessionItem concessionItem);
        Task DeleteAsync(Guid id);
        Task<IEnumerable<ConcessionItem>> GetActiveItemsAsync();
    }
}

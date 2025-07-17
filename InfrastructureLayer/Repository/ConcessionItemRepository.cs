using DomainLayer.Entities;
using InfrastructureLayer.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace InfrastructureLayer.Repository
{
    public class ConcessionItemRepository : IConcessionItemRepository
    {
        private readonly MovieContext _context;

        public ConcessionItemRepository(MovieContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ConcessionItem>> GetAllAsync()
        {
            return await _context.ConcessionItems.ToListAsync();
        }

        public async Task<ConcessionItem> GetByIdAsync(Guid id)
        {
            return await _context.ConcessionItems.FindAsync(id);
        }

        public async Task<ConcessionItem> CreateAsync(ConcessionItem concessionItem)
        {
            _context.ConcessionItems.Add(concessionItem);
            await _context.SaveChangesAsync();
            return concessionItem;
        }

        public async Task UpdateAsync(ConcessionItem concessionItem)
        {
            _context.Entry(concessionItem).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var concessionItem = await _context.ConcessionItems.FindAsync(id);
            if (concessionItem != null)
            {
                _context.ConcessionItems.Remove(concessionItem);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<ConcessionItem>> GetActiveItemsAsync()
        {
            return await _context.ConcessionItems
                .Where(item => item.IsActive)
                .ToListAsync();
        }
    }
}

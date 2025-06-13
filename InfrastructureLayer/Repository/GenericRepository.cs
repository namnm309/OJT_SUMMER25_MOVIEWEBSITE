using System.Linq.Expressions;
using InfrastructureLayer.Data;
using Microsoft.EntityFrameworkCore;

namespace InfrastructureLayer.Repository
{
    // Generic Repository Implementation - triển khai interface cho tất cả entities
    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        protected readonly MovieContext _context;
        protected readonly DbSet<T> _dbSet;

        public GenericRepository(MovieContext context)
        {
            _context = context;
            _dbSet = context.Set<T>(); // Set<T>() = DbSet cho entity type T
        }

        // === BASIC CRUD OPERATIONS ===

        public async Task<T> CreateAsync(T entity)
        {
            // Thêm entity vào DbSet
            await _dbSet.AddAsync(entity);
            // Lưu thay đổi vào database
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task<List<T>> GetAllAsync()
        {
            // Lấy tất cả entities từ DbSet
            return await _dbSet.ToListAsync();
        }

        public async Task<T?> GetByIdAsync(Guid id)
        {
            // Tìm entity theo primary key (ID)
            return await _dbSet.FindAsync(id);
        }

        public async Task<T> UpdateAsync(T entity)
        {
            // Đánh dấu entity đã thay đổi
            _dbSet.Update(entity);
            // Lưu thay đổi
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            // Tìm entity cần xóa
            var entity = await GetByIdAsync(id);
            if (entity == null)
            {
                return false; // Không tìm thấy entity
            }

            // Xóa entity
            _dbSet.Remove(entity);
            await _context.SaveChangesAsync();
            return true;
        }

        // === QUERY OPERATIONS ===

        public async Task<List<T>> FindAsync(Expression<Func<T, bool>> predicate)
        {
            // Tìm entities theo điều kiện (lambda expression)
            // Ví dụ: await FindAsync(u => u.IsActive == true)
            return await _dbSet.Where(predicate).ToListAsync();
        }

        public async Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate)
        {
            // Tìm entity đầu tiên theo điều kiện, return null nếu không có
            return await _dbSet.FirstOrDefaultAsync(predicate);
        }

        public async Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null)
        {
            // Đếm entities, có thể có điều kiện hoặc không
            if (predicate == null)
            {
                return await _dbSet.CountAsync(); // Đếm tất cả
            }
            return await _dbSet.CountAsync(predicate); // Đếm theo điều kiện
        }

        public async Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate)
        {
            // Check có entity nào thoả điều kiện không
            return await _dbSet.AnyAsync(predicate);
        }

        // === ADVANCED OPERATIONS ===

        public async Task<List<T>> GetPagedAsync(int page, int pageSize)
        {
            // Pagination: bỏ qua (page-1)*pageSize record đầu, lấy pageSize records
            // Page bắt đầu từ 1, không phải 0
            return await _dbSet
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<List<T>> FindPagedAsync(Expression<Func<T, bool>> predicate, int page, int pageSize)
        {
            // Tìm theo điều kiện + pagination
            return await _dbSet
                .Where(predicate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }
    }
} 
using System.Linq.Expressions;

namespace InfrastructureLayer.Repository
{
    // Generic Repository - interface chung cho tất cả entities
    // T = Type của entity (như Users, Movie, Booking...)
    public interface IGenericRepository<T> where T : class
    {
        // === BASIC CRUD OPERATIONS ===
        
        // Tạo mới 1 entity
        Task<T> CreateAsync(T entity);
        
        // Lấy tất cả entities  
        Task<List<T>> GetAllAsync();
        
        // Tìm entity theo ID
        Task<T?> GetByIdAsync(Guid id);
        
        // Cập nhật entity
        Task<T> UpdateAsync(T entity);
        
        // Xóa entity theo ID
        Task<bool> DeleteAsync(Guid id);
        
        // === QUERY OPERATIONS ===
        
        // Tìm entities theo điều kiện
        // Expression<Func<T, bool>> = lambda expression như: x => x.IsActive == true
        Task<List<T>> FindAsync(Expression<Func<T, bool>> predicate);
        
        // Tìm 1 entity đầu tiên theo điều kiện
        Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate);
        
        // Đếm số lượng entities theo điều kiện
        Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null);
        
        // Check entity có tồn tại không
        Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate);
        
        // === ADVANCED OPERATIONS ===
        
        // Lấy với pagination (phân trang)
        Task<List<T>> GetPagedAsync(int page, int pageSize);
        
        // Tìm với pagination
        Task<List<T>> FindPagedAsync(Expression<Func<T, bool>> predicate, int page, int pageSize);
    }
} 
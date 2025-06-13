using DomainLayer.Entities;

namespace InfrastructureLayer.Repository
{
    // IUserRepository kế thừa IGenericRepository để có các methods cơ bản
    // + thêm các methods riêng cho Users
    public interface IUserRepository : IGenericRepository<Users>
    {
        // === USER-SPECIFIC METHODS ===
        // Các methods đặc biệt chỉ dành cho Users entity
        
        Task<Users?> GetByUsernameAsync(string username);
        Task<Users?> GetByEmailAsync(string email);
        Task<List<Users>> GetAllMembersAsync(); // Chỉ lấy users có role Member
        Task<bool> IsUsernameExistsAsync(string username);
        Task<bool> IsEmailExistsAsync(string email);
        
        // Các methods cơ bản như CreateAsync, UpdateAsync, GetByIdAsync 
        // đã có sẵn từ IGenericRepository<Users> rồi!
    }
} 
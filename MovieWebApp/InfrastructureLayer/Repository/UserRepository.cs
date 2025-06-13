using DomainLayer.Entities;
using DomainLayer.Enum;
using InfrastructureLayer.Data;
using Microsoft.EntityFrameworkCore;

namespace InfrastructureLayer.Repository
{
    // UserRepository kế thừa GenericRepository để có sẵn các methods cơ bản
    // + implement các methods đặc biệt cho Users
    public class UserRepository : GenericRepository<Users>, IUserRepository
    {
        // Constructor gọi constructor của GenericRepository
        public UserRepository(MovieContext context) : base(context)
        {
        }

        // === USER-SPECIFIC METHODS ===
        // Chỉ implement các methods đặc biệt, các methods cơ bản đã có từ GenericRepository

        public async Task<Users?> GetByUsernameAsync(string username)
        {
            // Sử dụng _dbSet từ GenericRepository
            return await _dbSet
                .FirstOrDefaultAsync(u => u.Username == username && u.IsActive);
        }

        public async Task<Users?> GetByEmailAsync(string email)
        {
            return await _dbSet
                .FirstOrDefaultAsync(u => u.Email == email && u.IsActive);
        }

        public async Task<List<Users>> GetAllMembersAsync()
        {
            // Chỉ lấy users có role Member và đang active
            return await _dbSet
                .Where(u => u.IsActive && u.Role == UserRole.Member)
                .OrderByDescending(u => u.CreatedAt)
                .ToListAsync();
        }

        public async Task<bool> IsUsernameExistsAsync(string username)
        {
            // Sử dụng ExistsAsync từ GenericRepository
            return await ExistsAsync(u => u.Username == username && u.IsActive);
        }

        public async Task<bool> IsEmailExistsAsync(string email)
        {
            return await ExistsAsync(u => u.Email == email && u.IsActive);
        }

        // Note: CreateAsync, UpdateAsync, GetByIdAsync đã có sẵn từ GenericRepository!
        // Không cần implement lại nữa 🎉
    }
} 